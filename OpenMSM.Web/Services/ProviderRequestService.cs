using AutoMapper;
using OpenMSM.Data;
using OpenMSM.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using OpenMSM.Web.ServiceDefinitions;
using System.ServiceModel;
using System.Threading.Tasks;
using www.openoandm.org.wsisbm;

namespace OpenMSM.Web.Services
{
    public class ProviderRequestService : ServiceBase, IProviderRequestService
    {
        public ProviderRequestService(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        public void CloseProviderRequestSession(string SessionID)
        {
            var session = CheckSession(SessionID, SessionType.Responder);
            session.IsClosed = true;

            // due to lazy loading not working in Entity Framework Core at time of coding, I am making an extra call here.
            var messages = this.appDbContext.Set<Message>().Where(m => m.CreatedBySessionId == session.Id).ToList();

            // expire every message for the session that is not already expired
            foreach (var message in messages.Where(m => !m.ExpiredByCreatorOn.HasValue))
            {
                message.ExpiredByCreatorOn = DateTime.UtcNow;
            }

            this.appDbContext.SaveChanges();
        }

        public Task CloseProviderRequestSessionAsync(string SessionID)
        {
            return Task.Factory.StartNew(() => CloseProviderRequestSession(SessionID));
        }

        [return: MessageParameter(Name = "SessionID")]
        public OpenProviderRequestSessionResponse OpenProviderRequestSession(OpenProviderRequestSessionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ChannelURI))
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("ChannelURI cannot be null or empty."), new FaultCode("Sender"), string.Empty);
            }
            var channel = GetChannelByUri(request.ChannelURI);
            if (channel == null)
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("A channel with the specified URI does not exist."), new FaultCode("Sender"), string.Empty);
            }
            if (!DoPermissionsMatchChannel(channel))
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("Provided header security token does not match the token assigned to the channel."), new FaultCode("Sender"), string.Empty);
            }
            if (channel.Type != OpenMSM.Data.Models.ChannelType.Request)
            {
                throw new FaultException<OperationFault>(new OperationFault(), new FaultReason("Channel type is not of type 'Request'."), new FaultCode("Sender"), string.Empty);
            }
            ValidateXPath(request.XPathExpression);

            var session = new Session
            {
                Type = SessionType.Responder,
                ChannelId = channel.Id,
                ListenerURI = request.ListenerURL,
                XPathExpression = request.XPathExpression,
                SessionNamespaces = request.XPathNamespace == null ? new SessionNamespace[0] : request.XPathNamespace.Select(m => new SessionNamespace
                {
                    Name = m.NamespaceName,
                    Prefix = m.NamespacePrefix
                }).ToArray(),
                SessionTopics = request.Topic == null ? new SessionTopic[0] : request.Topic.Select(m => new SessionTopic
                {
                    Topic = m
                }).ToArray()
            };

            appDbContext.Add(session);
            appDbContext.SaveChanges();
            return new OpenProviderRequestSessionResponse { SessionID = session.Id.ToString() };
        }

        public Task<OpenProviderRequestSessionResponse> OpenProviderRequestSessionAsync(OpenProviderRequestSessionRequest request)
        {
            return Task.Factory.StartNew(() => OpenProviderRequestSession(request));
        }

        public string PostResponse(string SessionID, string RequestMessageID, MessageContent MessageContent)
        {
            var session = CheckSession(SessionID, SessionType.Responder);

            // get the request session
            var requestingMessage = this.appDbContext.Set<Message>().Include(m => m.CreatedBySession).FirstOrDefault(m => m.Id == new Guid(RequestMessageID));

            // return nothing if the message is null or not of the correct type
            if (requestingMessage == null || requestingMessage.Type != MessageType.Request)
            {
                return string.Empty;
            }

            // create a message
            var message = new Message
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBySessionId = session.Id,
                RequestMessageId = requestingMessage.Id,
                Type = MessageType.Response,
                MessageBody = this.MessageContent.OuterXml,
                MessagesSessions = new[] { new MessagesSession { SessionId = requestingMessage.CreatedBySessionId } } // associate the message to the original requesting message sssion
            };

            this.appDbContext.Add(message);
            this.appDbContext.SaveChanges();

            return message.Id.ToString();
        }

        [return: MessageParameter(Name = "MessageID")]
        public Task<string> PostResponseAsync(string SessionID, string RequestMessageID, MessageContent MessageContent)
        {
            return Task.Factory.StartNew(() => PostResponse(SessionID, RequestMessageID, MessageContent));
        }

        public RequestMessage ReadRequest(string SessionID)
        {
            var session = CheckSession(SessionID, SessionType.Responder);

            // get the message session object that is a request
            var messageSession = GetNextMessageSession(session.Id, m => m.Message.Type == MessageType.Request);

            if (messageSession == null)
            {
                return null;
            }

            var content = new XmlDocument();
            content.LoadXml(messageSession.Message.MessageBody);
            var xmlElementContent = EvalateFilter(content.DocumentElement, session.SessionNamespaces.ToList(), session.XPathExpression);

            messageSession.MessageReadOn = DateTime.UtcNow; // mark the message as read
            appDbContext.SaveChanges(); // save changes now in case there is an error parsing the message body
            var intersectingTopic = session.SessionTopics.Select(m => m.Topic).Intersect(messageSession.Message.MessageTopics.Select(m => m.Topic)).FirstOrDefault(); // this is likely unecessary for this request/response
            return new RequestMessage
            {
                MessageContent = xmlElementContent,
                MessageID = messageSession.MessageId.ToString(),
                Topic = intersectingTopic
            };
        }

        [return: MessageParameter(Name = "RequestMessage")]
        public Task<RequestMessage> ReadRequestAsync(string SessionID)
        {
            return Task.Factory.StartNew(() => ReadRequest(SessionID));
        }

        public void RemoveRequest(string SessionID)
        {
            var session = CheckSession(SessionID, SessionType.Responder);

            // get the message session object that is a request
            var messageSession = GetNextMessageSession(session.Id, m => m.Message.Type == MessageType.Request);

            if (messageSession == null) // there were no requests to remove
            {
                return;
            }

            appDbContext.Remove(messageSession);
            appDbContext.SaveChanges();
        }

        public Task RemoveRequestAsync(string SessionID)
        {
            return Task.Factory.StartNew(() => RemoveRequest(SessionID));
        }
    }
}
