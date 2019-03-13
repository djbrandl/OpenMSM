using AutoMapper;
using ISBM.Data;
using ISBM.Data.Models;
using ISBM.ServiceDefinitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ISBM.Web.Services
{
    public abstract class ServiceBase
    {
        protected readonly IMapper mapper;
        protected readonly DbContext appDbContext;
        private string _accessToken { get; set; }
        public ServiceBase(DbContext dbContext, IMapper mapper)
        {
            this.mapper = mapper;
            this.appDbContext = dbContext;
        }

        public void SetAccessToken(string token)
        {
            this._accessToken = token;
        }

        protected string GetAccessToken()
        {
            return _accessToken;
        }

        protected ISBM.Data.Models.Channel GetChannelByUri(string channelUri)
        {
            return this.appDbContext.Set<ISBM.Data.Models.Channel>()
                            .Include(m => m.ChannelsSecurityTokens).ThenInclude(cst => cst.SecurityToken).FirstOrDefault(m => m.URI.ToLower() == channelUri.ToLower());
        }

        protected bool DoPermissionsMatchChannel(ISBM.Data.Models.Channel channel)
        {
            var permissionsToken = this.GetAccessToken();
            return !(channel.ChannelsSecurityTokens.Any() && !channel.ChannelsSecurityTokens.Select(m => m.SecurityToken?.Token).Contains(permissionsToken));
        }

        protected ISBM.Data.Models.Session GetSessionById(Guid sessionId)
        {
            return this.appDbContext.Set<ISBM.Data.Models.Session>().Include(m => m.SessionNamespaces).Include(m => m.Channel)
                .ThenInclude(m => m.ChannelsSecurityTokens).ThenInclude(cst => cst.SecurityToken).FirstOrDefault(m => m.Id == sessionId);
        }

        protected bool DoPermissionsMatchSession(ISBM.Data.Models.Session session)
        {
            var permissionsToken = this.GetAccessToken();
            return !(session.Channel.ChannelsSecurityTokens.Any() && !session.Channel.ChannelsSecurityTokens.Select(m => m.SecurityToken?.Token).Contains(permissionsToken));
        }

        protected ISBM.Data.Models.Session CheckSession(string sessionID, SessionType requiredType)
        {
            if (string.IsNullOrWhiteSpace(sessionID))
            {
                throw new SessionFaultException("SessionID cannot be null or empty.", new ArgumentNullException("SessionID"));
            }
            var session = GetSessionById(new Guid(sessionID));
            if (session == null)
            {
                throw new SessionFaultException("A session with the specified ID does not exist.");
            }
            if (session.Type != requiredType)
            {
                throw new SessionFaultException("The session specified is not of the correct type for this action.");
            }
            if (session.IsClosed)
            {
                throw new SessionFaultException("The session specified is closed.");
            }
            if (!DoPermissionsMatchSession(session))
            {
                throw new SessionFaultException("Provided header security token does not match the token assigned to the session's channel.");
            }
            return session;
        }
    }
}
