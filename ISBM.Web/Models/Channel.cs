using System.Runtime.Serialization;

namespace ISBM.Web.Models
{
    public class Channel
    {
        public string Uri { get; set; }
        public ServiceDefinitions.ChannelType Type { get; set; }
        public string Description { get; set; }

        // not required
        [DataMember]
        public SecurityToken[] SecurityTokens { get; set; }
    }
}
