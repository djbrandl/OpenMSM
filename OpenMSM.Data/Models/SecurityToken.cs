using System.Collections.Generic;

namespace OpenMSM.Data.Models
{
    public class SecurityToken : BaseEntity
    {
        public string Token { get; set; }

        public virtual ICollection<ChannelsSecurityTokens> ChannelsSecurityTokens { get; set; }
    }
}
