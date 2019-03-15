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

namespace ISBM.Web.Services
{
    public class ConsumerRequestService : ServiceBase, IConsumerRequestServiceSoap
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

        public void ExpireRequest(string SessionID, string MessageID)
        {
            throw new NotImplementedException();
        }

        public string OpenConsumerRequestSession(string ChannelURI, string ListenerURL)
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

        public string PostRequest(string SessionID, XmlElement MessageContent, string Topic, [XmlElement(DataType = "duration")] string Expiry)
        {
            var session = CheckSession(SessionID, SessionType.Requester);

            // get the responder sessions
            var responderSessions = this.appDbContext.Set<Session>().Include(m => m.SessionTopics)
                .Where(m => m.Type == SessionType.Responder && m.ChannelId == session.ChannelId).ToList();

            // filter the sessions so that we only get subscribers that have any of the same topics as the posted message
            responderSessions.RemoveAll(m => !m.SessionTopics.Select(v => v.Topic).Contains(Topic));
            
            DateTime? expiration = null;
            if (!string.IsNullOrWhiteSpace(Expiry))
            {
                var expirationTimeSpan = XmlConvert.ToTimeSpan(Expiry);
                expiration = DateTime.UtcNow.Add(expirationTimeSpan);
            }
            // create a message
            var message = new Message
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBySessionId = session.Id,
                ExpiresOn = expiration,
                Type = MessageType.Request,
                MessageBody = MessageContent.OuterXml
            };
            if (!string.IsNullOrWhiteSpace(Topic))
            {
                message.MessageTopics = new[] { new MessageTopic { Topic = Topic } };
            }

            // link responder sessions to the new message
            message.MessagesSessions = responderSessions.Select(m => new MessagesSession
            {
                SessionId = m.Id
            }).ToList();

            this.appDbContext.Add(message);
            this.appDbContext.SaveChanges();

            return message.Id.ToString();
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
    }
}
