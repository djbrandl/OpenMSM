using AutoMapper;
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
        public ConsumerPublicationService(DbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        #region Private Methods

        private XmlElement EvalateFilter(XmlElement messageContent, IList<ISBM.Data.Models.SessionNamespace> namespaces, string xPathExpression)
        {
            // If there is no filtering of message content
            if (string.IsNullOrWhiteSpace(xPathExpression) || namespaces == null || !namespaces.Any())
            {
                return messageContent;
            }

            var expr = System.Xml.XPath.XPathExpression.Compile(xPathExpression);
            var navigator = messageContent.OwnerDocument.CreateNavigator();
            foreach (var ns in namespaces)
            {
                var nsManager = new XmlNamespaceManager(navigator.NameTable);
                nsManager.AddNamespace(ns.Prefix, ns.Name);
            }
            switch (expr.ReturnType)
            {
                // If the expression evaluates to a boolean, return full message content if the boolean evaluted to true, otherwise return null
                case XPathResultType.Boolean:
                    return (bool)navigator.Evaluate(expr) ? messageContent : null;
                // If the expression evaluates to a number, return full message content if the number is greater than 0, otherwise return null
                // I am imagining a scenario where you are counting the number of child elements to filter as this scenario
                case XPathResultType.Number:
                    return (int)navigator.Evaluate(expr) > 0 ? messageContent : null;
                // If the expression evaluates to a string, return full message content if the string is not empty, otherwise return null
                case XPathResultType.String:
                    return string.IsNullOrWhiteSpace(navigator.Evaluate(expr).ToString()) ? null : messageContent;

                // If the expression evaluates to a node set, return full message content if the node set count is greater than 0, otherwise return null
                case XPathResultType.NodeSet:
                    var nodes = navigator.Select(expr);
                    return nodes.Count > 0 ? messageContent : null;
                default:
                    return null;
            }
        }

        private ISBM.Data.Models.MessagesSession GetNextMessageSession(Guid sessionId)
        {
            return appDbContext.Set<MessagesSession>().Include(m => m.Message)
               .OrderBy(m => m.Message.CreatedOn) // with the lowest created on date
               .FirstOrDefault(m =>
                   m.SessionId == sessionId // for the session
                   && !m.Message.ExpiredByCreatorOn.HasValue // that is not expired by the publisher 
                   && (!m.Message.ExpiresOn.HasValue || m.Message.ExpiresOn.Value >= DateTime.UtcNow)); // and has not expired by by the date that it was set on creation
        }

        #endregion

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

            if (!string.IsNullOrWhiteSpace(XPathExpression))
            {
                try
                {
                    var expr = System.Xml.XPath.XPathExpression.Compile(XPathExpression);
                    if (new XPathResultType[] { XPathResultType.Any, XPathResultType.Error, XPathResultType.Navigator }.Contains(expr.ReturnType))
                    {
                        throw new SessionFaultException("Current implementation does not handle XPathExpressions which return XPathResultType.Any/Error/Navigator.");
                    }
                }
                catch (XPathException)
                {
                    throw new SessionFaultException("Provided XPathExpression does not compile or is not a valid XPathExpression.");
                }
            }

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
            
            return new PublicationMessage
            {
                MessageContent = xmlElementContent,
                MessageID = messageSession.MessageId.ToString(),
                Topic = messageSession.Message.MessageTopics.Select(m => m.Topic).ToArray()
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
