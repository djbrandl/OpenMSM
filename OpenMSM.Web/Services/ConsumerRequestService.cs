using AutoMapper;
using OpenMSM.Data;
using OpenMSM.Data.Models;
using OpenMSM.Web.ServiceDefinitions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel;
using www.openoandm.org.wsisbm;

namespace OpenMSM.Web.Services
{
    public class ConsumerRequestService : ServiceBase, IConsumerRequestService
    {
        public ConsumerRequestService(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        public void CloseConsumerRequestSession(string SessionID)
        {
            var session = CheckSession(SessionID, SessionType.Requester);
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

        public Task CloseConsumerRequestSessionAsync(string SessionID)
        {
            return Task.Factory.StartNew(() => CloseConsumerRequestSession(SessionID));
        }

        public void ExpireRequest(string SessionID, string MessageID)
        {
            var session = CheckSession(SessionID, SessionType.Requester);
            var message = this.appDbContext.Set<Message>().FirstOrDefault(m => m.CreatedBySessionId == session.Id && m.Id == new Guid(MessageID));
            if (message == null)
            {
                return;
            }

            message.ExpiredByCreatorOn = DateTime.UtcNow;
            appDbContext.SaveChanges();
        }

        public Task ExpireRequestAsync(string SessionID, string MessageID)
        {
            return Task.Factory.StartNew(() => ExpireRequest(SessionID, MessageID));
        }

        public string OpenConsumerRequestSession(string ChannelURI, string ListenerURL)
        {
            if (string.IsNullOrWhiteSpace(ChannelURI))
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("ChannelURI cannot be null or empty."), new FaultCode("Sender"), string.Empty);
            }
            var channel = GetChannelByUri(ChannelURI);
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
            var session = new Session
            {
                ChannelId = channel.Id,
                Type = SessionType.Requester,
                ListenerURI = ListenerURL
            };

            this.appDbContext.Set<Session>().Add(session);
            this.appDbContext.SaveChanges();
            return session.Id.ToString();
        }

        [return: MessageParameter(Name = "SessionID")]
        public Task<string> OpenConsumerRequestSessionAsync(string ChannelURI, string ListenerURL)
        {
            return Task.Factory.StartNew(() => OpenConsumerRequestSession(ChannelURI, ListenerURL));
        }
        
        [return: MessageParameter(Name = "MessageID")]
        public PostRequestResponse PostRequest(PostRequestRequest request)
        {
            var session = CheckSession(request.SessionID, SessionType.Requester);

            // get the responder sessions
            var responderSessions = this.appDbContext.Set<Session>().Include(m => m.SessionTopics)
                .Where(m => m.Type == SessionType.Responder && m.ChannelId == session.ChannelId).ToList();

            // filter the sessions so that we only get subscribers that have any of the same topics as the posted message
            responderSessions.RemoveAll(m => !m.SessionTopics.Select(v => v.Topic).Contains(request.Topic));

            DateTime? expiration = null;
            if (!string.IsNullOrWhiteSpace(request.Expiry))
            {
                var expirationTimeSpan = XmlConvert.ToTimeSpan(request.Expiry);
                expiration = DateTime.UtcNow.Add(expirationTimeSpan);
            }
            // create a message
            var message = new Message
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBySessionId = session.Id,
                ExpiresOn = expiration,
                Type = MessageType.Request,
                MessageBody = request.MessageContent.OuterXml
            };
            if (!string.IsNullOrWhiteSpace(request.Topic))
            {
                message.MessageTopics = new[] { new MessageTopic { Topic = request.Topic } };
            }

            // link responder sessions to the new message
            message.MessagesSessions = responderSessions.Select(m => new MessagesSession
            {
                SessionId = m.Id
            }).ToList();

            this.appDbContext.Add(message);
            this.appDbContext.SaveChanges();

            var response = new PostRequestResponse
            {
                MessageID = message.Id.ToString()
            };
            return response;
        }

        public Task<PostRequestResponse> PostRequestAsync(PostRequestRequest request)
        {
            return Task.Factory.StartNew(() => PostRequest(request));
        }

        public ResponseMessage ReadResponse(string SessionID, string RequestMessageID)
        {
            var session = CheckSession(SessionID, SessionType.Requester);

            // get the message session object
            var messageSession = GetNextMessageSession(session.Id, m => m.Message.Type == MessageType.Response && m.Message.RequestMessageId == new Guid(RequestMessageID));

            if (messageSession == null)
            {
                return null;
            }

            var content = new XmlDocument();
            content.LoadXml(messageSession.Message.MessageBody);

            messageSession.MessageReadOn = DateTime.UtcNow; // mark the message as read
            appDbContext.SaveChanges(); // save changes now in case there is an error parsing the message body

            return new ResponseMessage
            {
                MessageContent = content.DocumentElement,
                MessageID = messageSession.MessageId.ToString()
            };
        }

        [return: MessageParameter(Name = "ResponseMessage")]
        public Task<ResponseMessage> ReadResponseAsync(string SessionID, string RequestMessageID)
        {
            return Task.Factory.StartNew(() => ReadResponse(SessionID, RequestMessageID));
        }

        public void RemoveResponse(string SessionID, string RequestMessageID)
        {
            var session = CheckSession(SessionID, SessionType.Requester);

            // get the message session object that is a request
            var messageSession = GetNextMessageSession(session.Id, m => m.Message.Type == MessageType.Response && m.Message.RequestMessageId == new Guid(RequestMessageID));

            if (messageSession == null) // there were no requests to remove
            {
                return;
            }

            appDbContext.Remove(messageSession);
            appDbContext.SaveChanges();
        }

        public Task RemoveResponseAsync(string SessionID, string RequestMessageID)
        {
            return Task.Factory.StartNew(() => RemoveResponse(SessionID, RequestMessageID));
        }
    }
}
