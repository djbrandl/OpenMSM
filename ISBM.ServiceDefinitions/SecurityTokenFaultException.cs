using System;
using System.Runtime.Serialization;

namespace ISBM.ServiceDefinitions
{

    [DataContract(Namespace = "http://www.openoandm.org/xml/ISBM/")]
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