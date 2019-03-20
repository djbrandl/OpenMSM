using System.Runtime.Serialization;

namespace OpenMSM.Web.Models
{
    public class Channel
    {
        public string Uri { get; set; }
        public ChannelType Type { get; set; }
        public string Description { get; set; }

        // not required
        [DataMember]
        public SecurityToken[] SecurityTokens { get; set; }
    }
}
