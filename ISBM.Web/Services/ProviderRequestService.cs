using ISBM.ServiceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ISBM.Web.Services
{
    public class ProviderRequestService : IProviderRequestServiceSoap
    {
        public void CloseProviderRequestSession(string SessionID)
        {
            throw new NotImplementedException();
        }

        public string OpenProviderRequestSession(string ChannelURI, [XmlElement("Topic")] string[] Topic, string ListenerURL, string XPathExpression, [XmlElement("XPathNamespace")] Namespace[] XPathNamespace)
        {
            throw new NotImplementedException();
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
