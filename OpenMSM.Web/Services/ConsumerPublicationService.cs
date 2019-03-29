using AutoMapper;
using OpenMSM.Data;
using OpenMSM.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using OpenMSM.Web.ServiceDefinitions;
using System.ServiceModel;
using www.openoandm.org.wsisbm;

namespace OpenMSM.Web.Services
{
    public class ConsumerPublicationService : ServiceBase, IConsumerPublicationService
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

        public Task CloseSubscriptionSessionAsync(string SessionID)
        {
            return Task.Factory.StartNew(() => CloseSubscriptionSession(SessionID));
        }
        
        [return: MessageParameter(Name = "SessionID")]
        public OpenSubscriptionSessionResponse OpenSubscriptionSession(OpenSubscriptionSessionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ChannelURI))
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("ChannelURI cannot be null or empty."), new FaultCode("Sender"), string.Empty);
            }
            var channel = GetChannelByUri(request.ChannelURI);
            if (channel == null)
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("A channel with the specified URI does not exist."), new FaultCode("Sender"), string.Empty);
            }
            if (!DoPermissionsMatchChannel(channel))
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("Provided header security token does not match the token assigned to the channel."), new FaultCode("Sender"), string.Empty);
            }
            if (channel.Type != OpenMSM.Data.Models.ChannelType.Publication)
            {
                throw new FaultException<OperationFault>(new OperationFault(), new FaultReason("Channel type is not of type \"Publication\"."), new FaultCode("Sender"), string.Empty);
            }
            ValidateXPath(request.XPathExpression);

            var session = new Session
            {
                Type = SessionType.Subscriber,
                ChannelId = channel.Id,
                ListenerURI = request.ListenerURL,
                XPathExpression = request.XPathExpression,
                SessionNamespaces = request.XPathNamespace == null ? new SessionNamespace[0] : request.XPathNamespace.Select(m => new SessionNamespace
                {
                    Name = m.NamespaceName,
                    Prefix = m.NamespacePrefix
                }).ToArray(),
                SessionTopics = request.Topic == null ? new SessionTopic[0] : request.Topic.Select(m => new SessionTopic
                {
                    Topic = m
                }).ToArray()
            };

            appDbContext.Add(session);
            appDbContext.SaveChanges();
            return new OpenSubscriptionSessionResponse { SessionID = session.Id.ToString() };
        }

        public Task<OpenSubscriptionSessionResponse> OpenSubscriptionSessionAsync(OpenSubscriptionSessionRequest request)
        {
            return Task.Factory.StartNew(() => OpenSubscriptionSession(request));
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

        [return: MessageParameter(Name = "PublicationMessage")]
        public Task<PublicationMessage> ReadPublicationAsync(string SessionID)
        {
            return Task.Factory.StartNew(() => ReadPublication(SessionID));
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

        public Task RemovePublicationAsync(string SessionID)
        {
            return Task.Factory.StartNew(() => RemovePublication(SessionID));
        }
    }
}
