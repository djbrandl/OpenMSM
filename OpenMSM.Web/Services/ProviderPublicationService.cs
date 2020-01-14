using AutoMapper;
using OpenMSM.Data;
using OpenMSM.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using OpenMSM.Web.ServiceDefinitions;
using System.ServiceModel;
using System.Threading.Tasks;
using www.openoandm.org.wsisbm;

namespace OpenMSM.Web.Services
{
    public class ProviderPublicationService : ServiceBase, IProviderPublicationService
    {
        public ProviderPublicationService(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        public void ClosePublicationSession(string SessionID)
        {
            var session = CheckSession(SessionID, SessionType.Publisher);
            session.IsClosed = true;

            // due to lazy loading not working in Entity Framework Core at time of coding, I am making an extra call here.
            var messages = this.appDbContext.Set<Message>().Where(m => m.CreatedBySessionId == session.Id).ToList();

            // expire every message for the session that is not already expired
            foreach (var message in messages.Where(m => !m.ExpiredByCreatorOn.HasValue))
            {
                message.ExpiredByCreatorOn = DateTime.UtcNow;
            }

            this.appDbContext.SaveChanges();
        }

        public Task ClosePublicationSessionAsync(string SessionID)
        {
            return Task.Factory.StartNew(() => ClosePublicationSession(SessionID));
        }

        public void ExpirePublication(string SessionID, string MessageID)
        {
            var session = CheckSession(SessionID, SessionType.Publisher);
            var message = this.appDbContext.Set<Message>().FirstOrDefault(m => m.CreatedBySessionId == session.Id && m.Id == new Guid(MessageID));
            if (message == null)
            {
                return;
            }

            message.ExpiredByCreatorOn = DateTime.UtcNow;
            appDbContext.SaveChanges();
        }

        public Task ExpirePublicationAsync(string SessionID, string MessageID)
        {
            return Task.Factory.StartNew(() => ExpirePublication(SessionID, MessageID));
        }

        public string OpenPublicationSession(string ChannelURI)
        {
            if (string.IsNullOrWhiteSpace(ChannelURI))
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("ChannelURI cannot be null or empty."), new FaultCode("Sender"), string.Empty);
            }
            var channel = GetChannelByUri(ChannelURI);
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
                throw new FaultException<OperationFault>(new OperationFault(), new FaultReason("Channel type is not of type 'Publication'."), new FaultCode("Sender"), string.Empty);
            }
            var session = new Session
            {
                ChannelId = channel.Id,
                Type = SessionType.Publisher
            };
            this.appDbContext.Set<Session>().Add(session);
            this.appDbContext.SaveChanges();
            return session.Id.ToString();
        }

        [return: MessageParameter(Name = "SessionID")]
        public Task<string> OpenPublicationSessionAsync(string ChannelURI)
        {
            return Task.Factory.StartNew(() => OpenPublicationSession(ChannelURI));
        }

        [return: MessageParameter(Name = "MessageID")]
        public PostPublicationResponse PostPublication(PostPublicationRequest request)
        {
            var session = CheckSession(request.SessionID, SessionType.Publisher);

            // get all subscribers for this session
            var subscriberSessions = this.appDbContext.Set<Session>().Include(m => m.SessionTopics)
                .Where(m => m.Type == SessionType.Subscriber && m.ChannelId == session.ChannelId).ToList();

            // filter the sessions so that we only get subscribers that have any of the same topics as the posted message
            subscriberSessions = subscriberSessions.Where(m => m.SessionTopics.Select(v => v.Topic).Intersect(request.Topic).Any()).ToList();
            DateTime? expiration = null;
            if (!string.IsNullOrWhiteSpace(request.Expiry))
            {
                var expirationTimeSpan = XmlConvert.ToTimeSpan(request.Expiry);
                expiration = DateTime.UtcNow.Add(expirationTimeSpan);
            }
            string messageBody;
            string contentType;
            switch (request.MessageContent)
            {
                case XMLContent x:
                    messageBody = x.ToString();
                    contentType = "application/xml";
                    break;
                case StringContent s:
                    messageBody = s.ToString();
                    contentType = "application/text";
                    break;
                case BinaryContent b:
                    messageBody = b.ToString();
                    contentType = b.mediaType;
                    break;
                default:
                    messageBody = request.MessageContent.ToString();
                    contentType = "Unknown";
                    break;
            }
            // create a message
            var message = new Message
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBySessionId = session.Id,
                ExpiresOn = expiration,
                Type = MessageType.Publication,
                MessageBody = messageBody,
                ContentType = contentType,
                MessageTopics = request.Topic.Select(m =>
                    new MessageTopic
                    {
                        Topic = m
                    }).ToList()
            };

            // link subscriber sessions to the new message
            message.MessagesSessions = subscriberSessions.Select(m => new MessagesSession
            {
                SessionId = m.Id
            }).ToList();

            this.appDbContext.Add(message);
            this.appDbContext.SaveChanges();

            return new PostPublicationResponse { MessageID = message.Id.ToString() };
        }

        public Task<PostPublicationResponse> PostPublicationAsync(PostPublicationRequest request)
        {
            return Task.Factory.StartNew(() => PostPublication(request));
        }
    }
}
