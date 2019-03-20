namespace OpenMSM.Web.Models
{

    public class Message
    {
        public string Id { get; set; }
        public MessageType Type { get; set; }
        public string Content { get; set; }
        public string Duration { get; set; }
        public string[] Topics { get; set; }
    }
}