namespace ISBM.Data.Models
{
    using System.Collections.Generic;

    public partial class Session : BaseEntity
    {
        public Session()
        {
            this.Messages = new HashSet<Message>();
            this.MessagesSessions = new HashSet<MessagesSession>();
            this.SessionTopics = new HashSet<SessionTopic>();
        } 
        public System.Guid ChannelId { get; set; }
        public int Type { get; set; }
        public string ListenerURI { get; set; }
    
        public virtual Channel Channel { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<MessagesSession> MessagesSessions { get; set; }
        public virtual ICollection<SessionTopic> SessionTopics { get; set; }
    }
}
