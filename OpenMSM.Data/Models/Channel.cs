namespace OpenMSM.Data.Models
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    public partial class Channel : BaseEntity
    {    
        public string URI { get; set; }
        public ChannelType Type { get; set; }
        public string Description { get; set; }
        
        public virtual ICollection<ChannelsSecurityTokens> ChannelsSecurityTokens { get; set; }
        public virtual ICollection<Session> Sessions { get; set; }
    }
}
