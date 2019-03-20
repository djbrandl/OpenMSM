namespace OpenMSM.Data.Models
{
    public enum SessionType
    {
        Publisher, Subscriber, Requester, Responder
    }
    public enum ChannelType
    {
        Publication,
        Request,
    }
    public enum MessageType
    {
        Request, Response, Publication
    }
}