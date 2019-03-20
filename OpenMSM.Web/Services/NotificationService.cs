using AutoMapper;
using OpenMSM.Data;
using OpenMSM.ServiceDefinitions;
using OpenMSM.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenMSM.Web.Services
{
    public class NotificationService : INotificationServiceSoap
    {
        private IHttpClientFactory HttpClientFactory { get; set; }
        private AppDbContext AppDbContext { get; set; }

        public NotificationService(AppDbContext dbContext, IHttpClientFactory httpClientFactory)
        {
            this.AppDbContext = dbContext;
            this.HttpClientFactory = httpClientFactory;
        }

        public async Task<HttpResponseMessage[]> NotifyAllListeners(Guid creatorSessionId, Guid messageId)
        {
            var messageSessionsToNotify = this.AppDbContext.MessagesSessions
                .Include(m => m.Session).ThenInclude(m => m.SessionTopics)
                .Include(m => m.Message).ThenInclude(m => m.MessageTopics)
                .Where(m => m.Message.CreatedBySessionId == creatorSessionId && m.MessageId == messageId && !string.IsNullOrWhiteSpace(m.Session.ListenerURI));

            var notificationList = new List<Task<HttpResponseMessage>>();
            foreach (var messageSession in messageSessionsToNotify)
            {
                var notification = new Notification
                {
                    SessionID = messageSession.SessionId.ToString(),
                    MessageID = messageId.ToString()
                };
                if (messageSession.Session.Type != Data.Models.SessionType.Requester)
                {
                    var messageTopics = messageSession.Message.MessageTopics.Select(m => m.Topic);
                    var relevantTopics = messageSession.Session.SessionTopics.Select(m => m.Topic).Intersect(messageTopics);
                    notification.Topic = relevantTopics.ToArray();
                    notification.RequestMessageID = messageSession.Message.RequestMessageId.ToString();
                }

                var client = HttpClientFactory.CreateClient("DefaultHttpClient");
                notificationList.Add(client.PostAsJsonAsync(messageSession.Session.ListenerURI, notification));
            }

            var result = await Task.WhenAll(notificationList.ToArray());
            return result;
        }

        // THIS IS THE INTERFACE FOR WHAT IS PASSED BUT DOES NOT NEED TO BE IMPLEMENTED BY THE SERVICE PROVIDER. IT MUST BE IMPLEMENTED BY THE CLIENT
        public void NotifyListener(string SessionID, string MessageID, [XmlElement("Topic")] string[] Topic, string RequestMessageID)
        {
            throw new NotImplementedException();

            // check topic as it cannot be used for consumer request session response notification

            //RequestMessageID allows correlation with the original request and thus it MUST only be used for consumer request session response notification.
        }
    }
}
