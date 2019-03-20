using System;
using System.Runtime.Serialization;

namespace OpenMSM.ServiceDefinitions
{
    [DataContract(Namespace = "http://www.openoandm.org/xml/OpenMSM/")]
    public class SecurityTokenFaultException : Exception
    {
        public SecurityTokenFaultException(string message)
            : base(message)
        {

        }

        public SecurityTokenFaultException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}