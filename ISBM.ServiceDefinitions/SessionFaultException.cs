using System;
using System.Runtime.Serialization;

namespace ISBM.ServiceDefinitions
{
    [DataContract(Namespace = "http://www.openoandm.org/xml/ISBM/")]
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