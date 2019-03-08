using AutoMapper;
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
    public class ChannelManagementService : ServiceBase, IChannelManagementServiceSoap
    {
        public ChannelManagementService(DbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

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

        private void AssociateTokensToChannel(ISBM.Data.Models.Channel channel, IEnumerable<string> securityTokens)
        {
            var tokens = securityTokens.Distinct();
            var existingSecurityTokens = appDbContext.Set<ISBM.Data.Models.SecurityToken>().Include(m => m.ChannelsSecurityTokens).ToList();
            var tokensToLink = existingSecurityTokens.Where(m => tokens.Contains(m.Token) && !m.ChannelsSecurityTokens.Any(v => v.ChannelId == channel.Id));
            var tokensToAdd = tokens.Where(m => !existingSecurityTokens.Select(v => v.Token).Contains(m));
            foreach (var tokenToAdd in tokensToAdd)
            {
                channel.ChannelsSecurityTokens.Add(new Data.Models.ChannelsSecurityTokens
                {
                    SecurityToken = new Data.Models.SecurityToken
                    {
                        Token = tokenToAdd
                    }
                });
            }
            foreach (var tokenToLink in tokensToLink)
            {
                channel.ChannelsSecurityTokens.Add(new Data.Models.ChannelsSecurityTokens
                {
                    SecurityTokenId = tokenToLink.Id
                });
            }
        }
        #endregion

        public void AddSecurityTokens(string ChannelURI, [XmlElement("SecurityToken")] XmlElement[] SecurityToken)
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
            AssociateTokensToChannel(channel, SecurityToken.Select(m => m.OuterXml));
            appDbContext.SaveChanges();
        }

        public void CreateChannel(string ChannelURI, ChannelType ChannelType, string ChannelDescription, [XmlElement("SecurityToken")] XmlElement[] SecurityToken)
        {
            if (string.IsNullOrWhiteSpace(ChannelURI))
            {
                throw new ChannelFaultException("ChannelURI cannot be null or empty.", new ArgumentNullException("ChannelURI"));
            }
            if (GetChannelByUri(ChannelURI) != null)
            {
                throw new ChannelFaultException("A channel with the specified URI already exists.");
            }

            var channel = new ISBM.Data.Models.Channel
            {
                Type = (int)ChannelType,
                URI = ChannelURI,
                Description = ChannelDescription,
                ChannelsSecurityTokens = new List<ISBM.Data.Models.ChannelsSecurityTokens>()
            };

            AssociateTokensToChannel(channel, SecurityToken.Select(m => m.OuterXml));
            appDbContext.Add(channel);
            appDbContext.SaveChanges();
        }

        public void DeleteChannel(string ChannelURI)
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
            appDbContext.Remove(channel);
            appDbContext.SaveChanges();
        }

        public Channel GetChannel(string ChannelURI)
        {
            var channel = GetChannelByUri(ChannelURI);
            if (channel == null)
            {
                throw new ChannelFaultException("A channel with the specified URI does not exist.");
            }
            if (!DoPermissionsMatchChannel(channel))
            {
                throw new ChannelFaultException("Provided header security token does not match the token assigned to the channel.");
            }
            return mapper.Map<ISBM.ServiceDefinitions.Channel>(channel);
        }

        public Channel[] GetChannels()
        {
            var permissionToken = this.GetAccessToken();
            var channels = this.appDbContext.Set<ISBM.Data.Models.SecurityToken>().Where(m => m.Token == permissionToken).SelectMany(m => m.ChannelsSecurityTokens).Select(m => m.Channel);
            var noSecurityChannels = this.appDbContext.Set<ISBM.Data.Models.Channel>().Where(m => !m.ChannelsSecurityTokens.Any());
            return channels.Union(noSecurityChannels).Select(m => mapper.Map<ISBM.ServiceDefinitions.Channel>(m)).ToArray();
        }

        public void RemoveSecurityTokens(string ChannelURI, [XmlElement("SecurityToken")] XmlElement[] SecurityToken)
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
            // ensure all tokens exist and are associated to a channel
            if (!channel.ChannelsSecurityTokens.Any())
            {
                return;
            }
            var existingAssignedTokens = channel.ChannelsSecurityTokens.Select(m => m.SecurityToken.Token);
            var inputTokens = SecurityToken.Select(m => m.OuterXml);
            if (inputTokens.Count() != existingAssignedTokens.Intersect(inputTokens).Count())
            {
                throw new SecurityTokenFaultException("One or more of the provided security tokens are not assigned to the channel.");
            }
            foreach (var tokenLink in channel.ChannelsSecurityTokens.Where(m => inputTokens.Contains(m.SecurityToken.Token)))
            {
                channel.ChannelsSecurityTokens.Remove(tokenLink);
            }
            appDbContext.SaveChanges();
        }
    }
}
