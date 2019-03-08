using System;
using System.Runtime.Serialization;

namespace ISBM.ServiceDefinitions
{
    [DataContract(Namespace = "http://www.openoandm.org/xml/ISBM/")]
    public class OperationFaultException : Exception
    {
        public OperationFaultException(string message)
            : base(message)
        {

        }

        public OperationFaultException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}