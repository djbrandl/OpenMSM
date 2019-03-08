using System;

namespace ISBM.Data.Models
{
    public class ChannelsSecurityTokens
    {
        public Guid ChannelId { get; set; }
        public Guid SecurityTokenId { get; set; }

        public Channel Channel { get; set; }
        public SecurityToken SecurityToken { get; set; }
    }
}