using System;
using System.Runtime.Serialization;

namespace OpenMSM.ServiceDefinitions
{
    [DataContract(Namespace = "http://www.openoandm.org/xml/OpenMSM/")]
    public class ChannelFaultException : Exception
    {
        public ChannelFaultException(string message)
            : base(message)
        {

        }

        public ChannelFaultException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
