namespace OpenMSM.ServiceDefinitions
{
    public interface IProviderPublicationServiceSoap {
        
        string OpenPublicationSession(string ChannelURI);
        
        string PostPublication(string SessionID, System.Xml.XmlElement MessageContent, [System.Xml.Serialization.XmlElementAttribute("Topic")] string[] Topic, [System.Xml.Serialization.XmlElementAttribute(DataType="duration")] string Expiry);
        
        void ExpirePublication(string SessionID, string MessageID);
        
        void ClosePublicationSession(string SessionID);
    }
}
