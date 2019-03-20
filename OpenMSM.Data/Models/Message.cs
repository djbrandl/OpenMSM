namespace OpenMSM.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

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
        public DateTime? ExpiredByCreatorOn { get; set; }
        public string MessageBody { get; set; }
        public Guid? RequestMessageId { get; set; }

        [NotMapped]
        public bool IsExpired
        {
            get
            {
                if (ExpiredByCreatorOn.HasValue)
                {
                    return true;
                }
                return ExpiresOn.HasValue ? DateTime.UtcNow >= ExpiresOn.Value : false;
            }
        }

        public virtual Session CreatedBySession { get; set; }
        public virtual ICollection<MessagesSession> MessagesSessions { get; set; }
        public virtual ICollection<MessageTopic> MessageTopics { get; set; }
        public virtual ICollection<Message> ResponseMessages { get; set; }
        public virtual Message RequestMessage { get; set; }
    }
}
