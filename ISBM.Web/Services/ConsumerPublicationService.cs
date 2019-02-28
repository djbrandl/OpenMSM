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
    public class ConsumerPublicationService : ServiceBase, IConsumerPublicationServiceSoap
    {
        public ConsumerPublicationService(DbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        public void CloseSubscriptionSession(string SessionID)
        {
            var session = this.appDbContext.Set<Session>().FirstOrDefault(m => m.Id == new Guid(SessionID));
            if (session != null)
            {
                appDbContext.Remove(session);
                appDbContext.SaveChanges();
            }
        }

        public string OpenSubscriptionSession(string ChannelURI, [XmlElement("Topic")] string[] Topic, string ListenerURL, string XPathExpression, [XmlElement("XPathNamespace")] Namespace[] XPathNamespace)
        {
            if (Topic == null || !Topic.Any())
            {
                throw new ArgumentNullException("Topic");
            }
            var channel = this.appDbContext.Set<ISBM.Data.Models.Channel>().FirstOrDefault(m => m.URI.ToLower() == ChannelURI.ToLower());
            if (channel == null)
            {
                throw new ArgumentException("Invalid Channel URI. Channel does not exist.");
            }
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Type = (int)SessionType.Subscriber,
                ChannelId = channel.Id,
                ListenerURI = ListenerURL,
                SessionTopics = Topic == null ? new SessionTopic[0] : Topic.Select(m => new SessionTopic
                {
                    Id = Guid.NewGuid(),
                    Topic = m
                }).ToArray()
            };
            appDbContext.Add(session);
            appDbContext.SaveChanges();
            return session.Id.ToString();
        }

        public PublicationMessage ReadPublication(string SessionID)
        {
            var sessionId = new Guid(SessionID);
            var messageSession = appDbContext.Set<MessagesSession>().OrderBy(m => m.Message.CreatedOn)
                .FirstOrDefault(m => (!m.Message.ExpiresOn.HasValue || m.Message.ExpiresOn.Value >= DateTime.UtcNow) && m.SessionId == sessionId);
            if (messageSession == null)
            {
                return null;
            }
            messageSession.MessageReadOn = DateTime.UtcNow;
            appDbContext.SaveChanges();

            var content = new XmlDocument();
            content.LoadXml(messageSession.Message.MessageBody);
            return new PublicationMessage
            {
                MessageContent = content.DocumentElement,
                MessageID = messageSession.MessageId.ToString(),
                Topic = messageSession.Message.MessageTopics.Select(m => m.Topic).ToArray()
            };
        }

        public void RemovePublication(string SessionID)
        {
            throw new NotImplementedException();
        }
    }
}
