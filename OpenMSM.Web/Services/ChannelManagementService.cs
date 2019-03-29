using AutoMapper;
using OpenMSM.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel;
using OpenMSM.Web.ServiceDefinitions;
using www.openoandm.org.wsisbm;

namespace OpenMSM.Web.Services
{
    public class ChannelManagementService : ServiceBase, IChannelManagementService
    {
        public ChannelManagementService(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        #region Private Methods
        private void AssociateTokensToChannel(OpenMSM.Data.Models.Channel channel, IEnumerable<string> securityTokens)
        {
            var tokens = securityTokens.Distinct().Select(m => GetHashedToken(m));
            var existingSecurityTokens = appDbContext.Set<OpenMSM.Data.Models.SecurityToken>().Include(m => m.ChannelsSecurityTokens).ToList();
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

        public void DeleteChannel(string ChannelURI)
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
            appDbContext.Remove(channel);
            appDbContext.SaveChanges();
        }

        public Channel GetChannel(string ChannelURI)
        {
            var channel = GetChannelByUri(ChannelURI);
            if (channel == null)
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("A channel with the specified URI already exists."), new FaultCode("Sender"), string.Empty);
            }
            if (!DoPermissionsMatchChannel(channel))
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("Provided header security token does not match the token assigned to the channel."), new FaultCode("Sender"), string.Empty);
            }
            return mapper.Map<OpenMSM.Web.ServiceDefinitions.Channel>(channel);
        }

        public Channel[] GetChannels()
        {
            var permissionToken = this.GetAccessToken();
            var channels = this.appDbContext.Set<OpenMSM.Data.Models.SecurityToken>().Where(m => m.Token == permissionToken).SelectMany(m => m.ChannelsSecurityTokens).Select(m => m.Channel);
            var noSecurityChannels = this.appDbContext.Set<OpenMSM.Data.Models.Channel>().Where(m => !m.ChannelsSecurityTokens.Any());
            return channels.Union(noSecurityChannels).Select(m => mapper.Map<OpenMSM.Web.ServiceDefinitions.Channel>(m)).ToArray();
        }

        public CreateChannelResponse CreateChannel(CreateChannelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ChannelURI))
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("ChannelURI cannot be null or empty."), new FaultCode("Sender"), string.Empty);
            }
            if (GetChannelByUri(request.ChannelURI) != null)
            {
                throw new FaultException<ChannelFault>(new ChannelFault(), new FaultReason("A channel with the specified URI already exists."), new FaultCode("Sender"), string.Empty);
            }

            var channel = new OpenMSM.Data.Models.Channel
            {
                Type = request.ChannelType == ServiceDefinitions.ChannelType.Publication ? Data.Models.ChannelType.Publication : Data.Models.ChannelType.Request,
                URI = request.ChannelURI,
                Description = request.ChannelDescription,
                ChannelsSecurityTokens = new List<OpenMSM.Data.Models.ChannelsSecurityTokens>()
            };

            AssociateTokensToChannel(channel, request.SecurityToken.Select(m => m.OuterXml));
            appDbContext.Add(channel);
            appDbContext.SaveChanges();
            return new CreateChannelResponse();
        }

        public Task<CreateChannelResponse> CreateChannelAsync(CreateChannelRequest request)
        {
            return Task.Factory.StartNew(() => CreateChannel(request));
        }

        public AddSecurityTokensResponse AddSecurityTokens(AddSecurityTokensRequest request)
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
            AssociateTokensToChannel(channel, request.SecurityToken.Select(m => m.OuterXml));
            appDbContext.SaveChanges();
            return new AddSecurityTokensResponse();
        }

        public Task<AddSecurityTokensResponse> AddSecurityTokensAsync(AddSecurityTokensRequest request)
        {
            return Task.Factory.StartNew(() => AddSecurityTokens(request));
        }

        public RemoveSecurityTokensResponse RemoveSecurityTokens(RemoveSecurityTokensRequest request)
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
            // ensure all tokens exist and are associated to a channel
            if (!channel.ChannelsSecurityTokens.Any())
            {
                return new RemoveSecurityTokensResponse();
            }
            var existingAssignedTokens = channel.ChannelsSecurityTokens.Select(m => m.SecurityToken.Token);
            var inputTokens = request.SecurityToken.Select(m => GetHashedToken(m.OuterXml));
            if (inputTokens.Count() != existingAssignedTokens.Intersect(inputTokens).Count())
            {
                throw new FaultException<SecurityTokenFault>(new SecurityTokenFault(), new FaultReason("One or more of the provided security tokens are not assigned to the channel."), new FaultCode("Sender"), string.Empty);
            }
            foreach (var tokenLink in channel.ChannelsSecurityTokens.Where(m => inputTokens.Contains(m.SecurityToken.Token)).ToList())
            {
                channel.ChannelsSecurityTokens.Remove(tokenLink);
            }
            appDbContext.SaveChanges();
            return new RemoveSecurityTokensResponse();
        }

        public Task<RemoveSecurityTokensResponse> RemoveSecurityTokensAsync(RemoveSecurityTokensRequest request)
        {
            return Task.Factory.StartNew(() => RemoveSecurityTokens(request));
        }

        public Task DeleteChannelAsync(string ChannelURI)
        {
            return Task.Factory.StartNew(() => DeleteChannel(ChannelURI));
        }

        [return: MessageParameter(Name = "Channel")]
        public Task<Channel> GetChannelAsync(string ChannelURI)
        {
            return Task.Factory.StartNew(() => GetChannel(ChannelURI));
        }

        [return: MessageParameter(Name = "Channel")]
        public GetChannelsResponse GetChannels(GetChannelsRequest request)
        {
            var permissionToken = this.GetAccessToken();
            var channels = this.appDbContext.Set<OpenMSM.Data.Models.SecurityToken>().Where(m => m.Token == permissionToken).SelectMany(m => m.ChannelsSecurityTokens).Select(m => m.Channel);
            var noSecurityChannels = this.appDbContext.Set<OpenMSM.Data.Models.Channel>().Where(m => !m.ChannelsSecurityTokens.Any());
            return new GetChannelsResponse
            {
                Channel = channels.Union(noSecurityChannels).Select(m => mapper.Map<OpenMSM.Web.ServiceDefinitions.Channel>(m)).ToArray()
            };
        }

        public Task<GetChannelsResponse> GetChannelsAsync(GetChannelsRequest request)
        {
            return Task.Factory.StartNew(() => GetChannels(request));
        }
    }
}
