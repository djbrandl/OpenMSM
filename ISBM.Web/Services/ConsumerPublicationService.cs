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
            var messageSession = appDbContext.Set<MessagesSession>().Include(m => m.Message)
                .OrderBy(m => m.Message.CreatedOn) // with the lowest created on date
                .FirstOrDefault(m =>
                    m.SessionId == session.Id // for the session
                    && !m.Message.ExpiredByCreatorOn.HasValue // that is not expired by the publisher 
                    && (!m.Message.ExpiresOn.HasValue || m.Message.ExpiresOn.Value >= DateTime.UtcNow)); // and has not expired by by the date that it was set on creation
            if (messageSession == null)
            {
                return null;
            }

            var messageContent = ParseContent(messageSession.Message.MessageBody, session.SessionNamespaces.ToList(), session.XPathExpression);

            messageSession.MessageReadOn = DateTime.UtcNow; // mark the message as read
            appDbContext.SaveChanges(); // save changes now in case there is an error parsing the message body

            // TODO: need to add filtering of xpath to respect content filtering

            return new PublicationMessage
            {
                MessageContent = messageContent,
                MessageID = messageSession.MessageId.ToString(),
                Topic = messageSession.Message.MessageTopics.Select(m => m.Topic).ToArray()
            };
        }

        private XmlElement ParseContent(string messageContent, IList<ISBM.Data.Models.SessionNamespace> namespaces, string xPathExpression)
        {
            var content = new XmlDocument();
            content.LoadXml(messageContent);
            var xmlElementContent = content.DocumentElement;
            var expr = System.Xml.XPath.XPathExpression.Compile(xPathExpression);

            if (string.IsNullOrWhiteSpace(xPathExpression) || namespaces == null || !namespaces.Any())
            {
                return xmlElementContent;
            }

            var builder = new StringBuilder();
            foreach (var ns in namespaces)
            {
                var navigator = content.CreateNavigator();
                var nsManager = new XmlNamespaceManager(navigator.NameTable);
                nsManager.AddNamespace(ns.Prefix, ns.Name);
                switch (expr.ReturnType)
                {
                    case XPathResultType.Number:
                    case XPathResultType.Boolean:
                    case XPathResultType.String:
                        builder.Append(navigator.Evaluate(expr).ToString());
                        break;
                    case XPathResultType.NodeSet:
                        var nodes = navigator.Select(expr);
                        while (nodes.MoveNext())
                        {
                            builder.Append(nodes.Current.OuterXml);
                        }
                        break;
                    default:
                        break;
                }
                if (builder.Length > 0)
                {
                    var result = new XmlDocument();
                    var root = result.CreateElement("root");
                    var rootContents = result.CreateTextNode(builder.ToString());
                    result.DocumentElement.AppendChild(root);
                    result.DocumentElement.LastChild.AppendChild(rootContents);
                    return result.DocumentElement;
                }
            }

            var defaultResult = new XmlDocument();
            var defaultRoot = defaultResult.CreateElement("root");
            defaultResult.DocumentElement.AppendChild(defaultRoot);
            return defaultResult.DocumentElement;
        }

        public void RemovePublication(string SessionID)
        {
            throw new NotImplementedException();
        }
    }
}
