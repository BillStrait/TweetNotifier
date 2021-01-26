using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dassanie.Models
{
    public class ClickSendInboundSMS
    {
        public string timestamp { get; set; }
        public string from { get; set; }
        public string body { get; set; }
        public string original_body { get; set; }
        public string original_message_id { get; set; }
        public string to { get; set; }
        public string custom_string { get; set; }
        public string message_id { get; set; }
        public string _keyword { get; set; }
    }

}
