using ISBM.ServiceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ISBM.Web.Services
{
    public class ProviderPublicationService : IProviderPublicationServiceSoap
    {
        public void ClosePublicationSession(string SessionID)
        {
            throw new NotImplementedException();
        }

        public void ExpirePublication(string SessionID, string MessageID)
        {
            throw new NotImplementedException();
        }

        public string OpenPublicationSession(string ChannelURI)
        {
            throw new NotImplementedException();
        }

        public string PostPublication(string SessionID, XmlElement MessageContent, [XmlElement("Topic")] string[] Topic, [XmlElement(DataType = "duration")] string Expiry)
        {
            throw new NotImplementedException();
        }
    }
}
