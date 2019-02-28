namespace ISBM.Data.Models
{
    public partial class SessionTopic : BaseEntity
    {
        public System.Guid SessionId { get; set; }
        public string Topic { get; set; }
    
        public virtual Session Session { get; set; }
    }
}
