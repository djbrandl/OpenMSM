using System.Collections.Generic;
namespace ISBM.Data.Models
{
    public partial class Session : BaseEntity
    {
        public System.Guid ChannelId { get; set; }
        public SessionType Type { get; set; }
        public string ListenerURI { get; set; }
        public string XPathExpression { get; set; }
        public bool IsClosed { get; set; }

        public virtual Channel Channel { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<MessagesSession> MessagesSessions { get; set; }
        public virtual ICollection<SessionTopic> SessionTopics { get; set; }
        public virtual ICollection<SessionNamespace> SessionNamespaces { get; set; }
    }
}
