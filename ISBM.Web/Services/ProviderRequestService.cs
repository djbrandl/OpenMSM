using AutoMapper;
using ISBM.Data;
using ISBM.Data.Models;
using ISBM.ServiceDefinitions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace ISBM.Web.Services
{
    public class ProviderRequestService : ServiceBase, IProviderRequestServiceSoap
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

        public string OpenProviderRequestSession(string ChannelURI, [XmlElement("Topic")] string[] Topic, string ListenerURL, string XPathExpression, [XmlElement("XPathNamespace")] Namespace[] XPathNamespace)
        {
            if (string.IsNullOrWhiteSpace(ChannelURI))
            {
                throw new ChannelFaultException("ChannelURI cannot be null or empty.", new ArgumentNullException("ChannelURI"));
            }
            var channel = GetChannelByUri(ChannelURI);
            if (channel == null)
            {
                throw new ChannelFaultException("A channel with the specified URI does not exist.");
            }
            if (!DoPermissionsMatchChannel(channel))
            {
                throw new ChannelFaultException("Provided header security token does not match the token assigned to the channel.");
            }
            if (channel.Type != ISBM.Data.Models.ChannelType.Request)
            {
                throw new OperationFaultException("Channel type is not of type 'Request'.");
            }
            ValidateXPath(XPathExpression);

            var session = new Session
            {
                Type = SessionType.Responder,
                ChannelId = channel.Id,
                ListenerURI = ListenerURL,
                XPathExpression = XPathExpression,
                SessionNamespaces = XPathNamespace == null ? new SessionNamespace[0] : XPathNamespace.Select(m => new SessionNamespace
                {
                    Name = m.NamespaceName,
                    Prefix = m.NamespacePrefix
                }).ToArray(),
                SessionTopics = Topic == null ? new SessionTopic[0] : Topic.Select(m => new SessionTopic
                {
                    Topic = m
                }).ToArray()
            };

            appDbContext.Add(session);
            appDbContext.SaveChanges();
            return session.Id.ToString();
        }

        public string PostResponse(string SessionID, string RequestMessageID, XmlElement MessageContent)
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
                MessageBody = MessageContent.OuterXml,
                MessagesSessions = new[] { new MessagesSession { SessionId = requestingMessage.CreatedBySessionId } } // associate the message to the original requesting message sssion
            };
            
            this.appDbContext.Add(message);
            this.appDbContext.SaveChanges();

            return message.Id.ToString();
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
    }
}
