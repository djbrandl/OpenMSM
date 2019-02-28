namespace ISBM.Data.Models
{

    public partial class MessageTopic : BaseEntity
    {
        public System.Guid MessageId { get; set; }
        public string Topic { get; set; }
    
        public virtual Message Message { get; set; }
    }
}
