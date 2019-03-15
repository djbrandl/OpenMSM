using AutoMapper;
using ISBM.Data;
using ISBM.Data.Models;
using ISBM.ServiceDefinitions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace ISBM.Web.Services
{
    public class ConsumerPublicationService : ServiceBase, IConsumerPublicationServiceSoap
    {
        public ConsumerPublicationService(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
        
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
            ValidateXPath(XPathExpression);

            var session = new Session
            {
                Type = SessionType.Subscriber,
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

        public PublicationMessage ReadPublication(string SessionID)
        {
            var session = CheckSession(SessionID, SessionType.Subscriber);

            // get the message session object
            var messageSession = GetNextMessageSession(session.Id);

            if (messageSession == null)
            {
                return null;
            }

            var content = new XmlDocument();
            content.LoadXml(messageSession.Message.MessageBody);
            var xmlElementContent = EvalateFilter(content.DocumentElement, session.SessionNamespaces.ToList(), session.XPathExpression);

            messageSession.MessageReadOn = DateTime.UtcNow; // mark the message as read
            appDbContext.SaveChanges(); // save changes now in case there is an error parsing the message body
            var intersectingTopics = session.SessionTopics.Select(m => m.Topic).Intersect(messageSession.Message.MessageTopics.Select(m => m.Topic));
            return new PublicationMessage
            {
                MessageContent = xmlElementContent,
                MessageID = messageSession.MessageId.ToString(),
                Topic = intersectingTopics.ToArray()
            };
        }

        public void RemovePublication(string SessionID)
        {
            var session = CheckSession(SessionID, SessionType.Subscriber);

            // get the message session object
            var messageSession = GetNextMessageSession(session.Id);

            if (messageSession == null) // there were no publications to remove
            {
                return;
            }

            appDbContext.Remove(messageSession);
            appDbContext.SaveChanges();
        }
    }
}
