namespace ISBM.Data.Models
{
    using System;

    public partial class MessagesSession
    {
        //[Key, Column(Order = 0)]
        public Guid MessageId { get; set; }
        //[Key, Column(Order = 1)]
        public Guid SessionId { get; set; }

        public DateTime? MessageReadOn { get; set; }
    
        public virtual Message Message { get; set; }
        public virtual Session Session { get; set; }
    }
}
