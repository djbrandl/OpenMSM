namespace OpenMSM.Web.Models
{
    public class MessageContent {
        public string MediaType { get; set; }
        public string ContentEncoding { get; set; }
        public object Content { get; set; }
    }
}