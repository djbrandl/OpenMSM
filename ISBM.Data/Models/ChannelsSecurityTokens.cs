using System;

namespace ISBM.Data.Models
{
    public class ChannelsSecurityTokens
    {
        public Guid ChannelId { get; set; }
        public Guid SecurityTokenId { get; set; }

        public virtual Channel Channel { get; set; }
        public virtual SecurityToken SecurityToken { get; set; }
    }
}