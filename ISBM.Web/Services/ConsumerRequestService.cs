using AutoMapper;
using ISBM.Data;
using ISBM.Data.Models;
using ISBM.ServiceDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ISBM.Web.Services
{
    public class ConsumerRequestService : ServiceBase, IConsumerRequestServiceSoap
    {
        public ConsumerRequestService(AppDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }

        public void CloseConsumerRequestSession(string SessionID)
        {
            throw new NotImplementedException();
        }

        public void ExpireRequest(string SessionID, string MessageID)
        {
            throw new NotImplementedException();
        }

        public string OpenConsumerRequestSession(string ChannelURI, string ListenerURL)
        {
            if (string.IsNullOrWhiteSpace(ChannelURI))
            {
                throw new ChannelFaultException("ChannelURI cannot be null or empty.", new ArgumentNullException("ChannelURI"));
            }
            var channel = GetChannelByUri(ChannelURI);
            if (channel == null)
            {
                throw new ChannelFaultException("A channel with the specified URI does not exist.");
            }
            if (!DoPermissionsMatchChannel(channel))
            {
                throw new ChannelFaultException("Provided header security token does not match the token assigned to the channel.");
            }
            if (channel.Type != ISBM.Data.Models.ChannelType.Request)
            {
                throw new OperationFaultException("Channel type is not of type 'Request'.");
            }
            var session = new Session
            {
                ChannelId = channel.Id,
                Type = SessionType.Requester,
                ListenerURI = ListenerURL
            };

            this.appDbContext.Set<Session>().Add(session);
            this.appDbContext.SaveChanges();
            return session.Id.ToString();            
        }

        public string PostRequest(string SessionID, XmlElement MessageContent, string Topic, [XmlElement(DataType = "duration")] string Expiry)
        {

            throw new NotImplementedException();
        }

        public ResponseMessage ReadResponse(string SessionID, string RequestMessageID)
        {
            throw new NotImplementedException();
        }

        public void RemoveResponse(string SessionID, string RequestMessageID)
        {
            throw new NotImplementedException();
        }
    }
}
