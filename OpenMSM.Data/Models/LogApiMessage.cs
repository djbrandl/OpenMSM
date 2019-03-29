using System;

namespace OpenMSM.Data.Models
{
    public class LogApiMessage : BaseEntity
    {
        public LogApiMessage()
        {

        }

        public string RequestIP { get; set; }
        public string RequestURL { get; set; }
        public string RequestMethod { get; set; }
        public string RequestBody { get; set; }
        public int ResponseStatus { get; set; }
        public string ResponseBody { get; set; }
        public DateTime RespondedOn { get; set; }
    }
}
