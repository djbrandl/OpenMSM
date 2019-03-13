using AutoMapper;
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
    public class ProviderPublicationService : ServiceBase, IProviderPublicationServiceSoap
    {
        public ProviderPublicationService(DbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
        
        public void ClosePublicationSession(string SessionID)
        {
            var session = CheckSession(SessionID, SessionType.Publisher);
            session.IsClosed = true;

            // due to lazy loading not working in Entity Framework Core at time of coding, I am making an extra call here.
            var messages = this.appDbContext.Set<Message>().Where(m => m.CreatedBySessionId == session.Id).ToList();

            // expire every message for the session that is not already expired
            foreach(var message in messages.Where(m => !m.ExpiredByCreatorOn.HasValue))
            {
                message.ExpiredByCreatorOn = DateTime.UtcNow;
            }

            this.appDbContext.SaveChanges();
        }

        public void ExpirePublication(string SessionID, string MessageID)
        {
            var session = CheckSession(SessionID, SessionType.Publisher);
            var message = this.appDbContext.Set<Message>().FirstOrDefault(m => m.CreatedBySessionId == session.Id);
            if (message == null)
            {
                return;
            }

            message.ExpiredByCreatorOn = DateTime.UtcNow;
            appDbContext.SaveChanges();
        }

        public string OpenPublicationSession(string ChannelURI)
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
            if (channel.Type != ISBM.Data.Models.ChannelType.Publication)
            {
                throw new OperationFaultException("Channel type is not of type \"Publication\".");
            }
            var session = new Session
            {
                ChannelId = channel.Id,
                Type = (int)SessionType.Publisher
            };
            this.appDbContext.Set<Session>().Add(session);
            this.appDbContext.SaveChanges();
            return session.Id.ToString();
        }

        public string PostPublication(string SessionID, XmlElement MessageContent, [XmlElement("Topic")] string[] Topic, [XmlElement(DataType = "duration")] string Expiry)
        {
            var session = CheckSession(SessionID, SessionType.Publisher);

            // get all subscribers for this session
            var subscriberSessions = this.appDbContext.Set<Session>().Include(m => m.SessionTopics)
                .Where(m => m.Type == SessionType.Subscriber && m.ChannelId == session.ChannelId).ToList();

            // filter the sessions so that we only get subscribers that have any of the same topics as the posted message
            subscriberSessions = subscriberSessions.Where(m => m.SessionTopics.Select(v => v.Topic).Intersect(Topic).Any()).ToList();

            // create a message
            var message = new Message
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBySessionId = session.Id,
                Type = MessageType.Publication,
                MessageBody = MessageContent.OuterXml,
                MessageTopics = Topic.Select(m =>
                    new MessageTopic
                    {
                        Topic = m
                    }).ToList()
            };

            // link subscriber sessions to the new message
            message.MessagesSessions = subscriberSessions.Select(m => new MessagesSession
            {
                SessionId = m.Id
            }).ToList();

            this.appDbContext.Add(message);
            this.appDbContext.SaveChanges();

            return message.Id.ToString();
        }
    }
}
