using AutoMapper;
using ISBM.Data.Models;
using ISBM.ServiceDefinitions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ISBM.Web.Services
{
    public class ProviderPublicationService : ServiceBase, IProviderPublicationServiceSoap
    {
        public ProviderPublicationService(DbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        #region Private Methods
        private ISBM.Data.Models.Channel GetChannelByUri(string channelUri)
        {
            return this.appDbContext.Set<ISBM.Data.Models.Channel>()
                            .Include(m => m.ChannelsSecurityTokens).ThenInclude(cst => cst.SecurityToken).FirstOrDefault(m => m.URI.ToLower() == channelUri.ToLower());
        }

        private bool DoPermissionsMatchChannel(ISBM.Data.Models.Channel channel)
        {
            var permissionsToken = this.GetAccessToken();
            return !(channel.ChannelsSecurityTokens.Any() && !channel.ChannelsSecurityTokens.Select(m => m.SecurityToken?.Token).Contains(permissionsToken));
        }

        private ISBM.Data.Models.Session GetSessionById(Guid sessionId)
        {
            return this.appDbContext.Set<ISBM.Data.Models.Session>()
                            .Include(m => m.Channel).ThenInclude(m => m.ChannelsSecurityTokens).ThenInclude(cst => cst.SecurityToken).FirstOrDefault(m => m.Id == sessionId);
        }

        private bool DoPermissionsMatchSession(ISBM.Data.Models.Session session)
        {
            var permissionsToken = this.GetAccessToken();
            return !(session.Channel.ChannelsSecurityTokens.Any() && !session.Channel.ChannelsSecurityTokens.Select(m => m.SecurityToken?.Token).Contains(permissionsToken));
        }

        #endregion

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
            var session = new Session
            {
                ChannelId = channel.Id,
                Type = (int)SessionType.Publisher
            };
            this.appDbContext.Set<Session>().Add(session);
            this.appDbContext.SaveChanges();
            return session.Id.ToString();
        }

        public string PostPublication(string SessionID, XmlElement MessageContent, [XmlElement("Topic")] string[] Topic, [XmlElement(DataType = "duration")] string Expiry)
        {
            if (string.IsNullOrWhiteSpace(SessionID))
            {
                throw new SessionFaultException("SessionID cannot be null or empty.", new ArgumentNullException("SessionID"));
            }
            var session = GetSessionById(new Guid(SessionID));
            if (session == null)
            {
                throw new SessionFaultException("A session with the specified ID does not exist.");
            }
            if (session.Type != SessionType.Publisher)
            {
                throw new SessionFaultException("The session specified is not a Publication session.");
            }
            if (!DoPermissionsMatchSession(session))
            {
                throw new SessionFaultException("Provided header security token does not match the token assigned to the session's channel.");
            }

            // get all subscribers for this session
            var subscriberSessions = this.appDbContext.Set<Session>()
                .Where(m => m.Type == SessionType.Subscriber && m.ChannelId == session.ChannelId)
                .Select(m => m.Id);

            // create a message
            var message = new Message
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBySessionId = session.Id,
                Type = MessageType.Publication,
                MessageBody = MessageContent.OuterXml,
                MessageTopics = Topic.Select(m =>
                    new MessageTopic
                    {
                        Topic = m
                    }).ToList()
            };

            // link subscriber sessions to the new message
            message.MessagesSessions = subscriberSessions.Select(m => new MessagesSession
            {
                SessionId = m
            }).ToList();

            this.appDbContext.Add(message);
            this.appDbContext.SaveChanges();

            return message.Id.ToString();
        }
    }
}
