namespace ISBM.Data.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Message : BaseEntity
    {
        public Message()
        {
            MessagesSessions = new HashSet<MessagesSession>();
            MessageTopics = new HashSet<MessageTopic>();
            ResponseMessages = new HashSet<Message>();
        }
    
        public Guid CreatedBySessionId { get; set; }
        public MessageType Type { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string MessageBody { get; set; }
        public Guid? RequestMessageId { get; set; }
    
        public virtual Session Session { get; set; }
        public virtual ICollection<MessagesSession> MessagesSessions { get; set; }
        public virtual ICollection<MessageTopic> MessageTopics { get; set; }
        public virtual ICollection<Message> ResponseMessages { get; set; }
        public virtual Message RequestMessage { get; set; }
    }
}
