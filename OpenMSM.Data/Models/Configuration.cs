namespace OpenMSM.Data.Models
{
    public class Configuration : BaseEntity
    {
        public bool StoreLogMessages { get; set; }
        public int NumberOfMessagesToStore { get; set; }
    }
}
