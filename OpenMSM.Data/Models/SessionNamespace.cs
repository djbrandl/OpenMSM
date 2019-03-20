namespace OpenMSM.Data.Models
{

    public partial class SessionNamespace : BaseEntity
    {
        public System.Guid SessionId { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }

        public Session Session { get; set; }
    }
} 