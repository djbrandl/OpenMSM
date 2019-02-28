using System.Collections.Generic;

namespace ISBM.Data.Models
{
    public class SecurityToken : BaseEntity
    {
        public string Token { get; set; }

        public virtual ICollection<ChannelsSecurityTokens> ChannelsSecurityTokens { get; set; }
    }
}
