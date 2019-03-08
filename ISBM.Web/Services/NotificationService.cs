using AutoMapper;
using ISBM.ServiceDefinitions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ISBM.Web.Services
{
    public class NotificationService : ServiceBase, INotificationServiceSoap
    {
        public NotificationService(DbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        public void NotifyListener(string SessionID, string MessageID, [XmlElement("Topic")] string[] Topic, string RequestMessageID)
        {
            // check topic as it cannot be used for consumer request session response notification

            //RequestMessageID allows correlation with the original request and thus it MUST only be used for consumer request session response notification.
        }
    }
}
