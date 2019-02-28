using ISBM.ServiceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ISBM.Web.Services
{
    public class ConsumerRequestService : IConsumerRequestServiceSoap
    {
        public void CloseConsumerRequestSession(string SessionID)
        {
            throw new NotImplementedException();
        }

        public void ExpireRequest(string SessionID, string MessageID)
        {
            throw new NotImplementedException();
        }

        public string OpenConsumerRequestSession(string ChannelURI, string ListenerURL)
        {
            throw new NotImplementedException();
        }

        public string PostRequest(string SessionID, XmlElement MessageContent, string Topic, [XmlElement(DataType = "duration")] string Expiry)
        {
            throw new NotImplementedException();
        }

        public ResponseMessage ReadResponse(string SessionID, string RequestMessageID)
        {
            throw new NotImplementedException();
        }

        public void RemoveResponse(string SessionID, string RequestMessageID)
        {
            throw new NotImplementedException();
        }
    }
}
