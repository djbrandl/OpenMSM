using AutoMapper;
using ISBM.Data;
using ISBM.Data.Models;
using ISBM.ServiceDefinitions;
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public RequestMessage ReadRequest(string SessionID)
        {
            throw new NotImplementedException();
        }

        public void RemoveRequest(string SessionID)
        {
            throw new NotImplementedException();
        }
    }
}
