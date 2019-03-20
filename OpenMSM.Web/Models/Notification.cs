using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMSM.Web.Models
{
    public class Notification
    {
        public string SessionID { get; set; }
        public string MessageID { get; set; }
        public string[] Topic { get; set; }
        public string RequestMessageID { get; set; }
    }
}
