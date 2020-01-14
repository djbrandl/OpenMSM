using OpenMSM.Web.Services;
using SoapCore;
using SoapCore.Extensibility;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace OpenMSM.Web.Middleware
{
    public class SOAPHeaderHandler : IMessageFilter
    {
        private List<ServiceBase> Services { get; }
        public SOAPHeaderHandler(ChannelManagementService channelManagementService,
            ProviderPublicationService providerPublicationService,
            ConsumerPublicationService consumerPublicationService,
            ProviderRequestService providerRequestService,
            ConsumerRequestService consumerRequestService)
        {
            Services = new List<ServiceBase>();
            Services.Add(channelManagementService);
            Services.Add(providerPublicationService);
            Services.Add(consumerPublicationService);
            Services.Add(providerRequestService);
            Services.Add(consumerRequestService);
        }
        public void OnRequestExecuting(Message message)
        {
            var wsUsernameToken = GetWsUsernameToken(message);
            if (wsUsernameToken == null)
            {
                return;
            }
            using (var memStm = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(WsUsernameToken));
                serializer.WriteObject(memStm, wsUsernameToken);

                memStm.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(memStm))
                {
                    string result = streamReader.ReadToEnd();
                    foreach (var service in Services)
                    {
                        service.SetAccessToken(result);
                    }
                }
            }
        }

        public void OnResponseExecuting(Message message)
        {
            // empty
        }

        private WsUsernameToken GetWsUsernameToken(Message message)
        {
            WsUsernameToken wsUsernameToken = null;
            for (var i = 0; i < message.Headers.Count; i++)
            {
                if (message.Headers[i].Name.ToLower() == "security")
                {
                    var reader = message.Headers.GetReaderAtHeader(i);
                    reader.Read();
                    var serializer = new DataContractSerializer(typeof(WsUsernameToken));
                    wsUsernameToken = (WsUsernameToken)serializer.ReadObject(reader, true);
                    reader.Close();
                }
            }

            return wsUsernameToken;
        }
    }
}
