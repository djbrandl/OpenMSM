using System.Runtime.Serialization;

namespace OpenMSM.Web.Models
{
    public class Session
    {
        public string Id { get; set; }
        public SessionType Type { get; set; }
        public string ListenerUrl { get; set; }
        public string[] Topics { get; set; }
        public string XPathExpression { get; set; }

        [DataMember]
        public XPathNamespace[] XPathNamespaces { get; set; }
    }
}