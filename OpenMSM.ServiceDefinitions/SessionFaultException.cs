using System;
using System.Runtime.Serialization;

namespace OpenMSM.ServiceDefinitions
{
    [DataContract(Namespace = "http://www.openoandm.org/xml/OpenMSM/")]
    public class SessionFaultException : Exception
    {
        public SessionFaultException(string message)
            : base(message)
        {

        }

        public SessionFaultException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}