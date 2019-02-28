using ISBM.ServiceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ISBM.Web.Services
{
    public class NotificationService : INotificationServiceSoap
    {
        public void NotifyListener(string SessionID, string MessageID, [XmlElement("Topic")] string[] Topic, string RequestMessageID)
        {
            throw new NotImplementedException();
        }
    }
}
