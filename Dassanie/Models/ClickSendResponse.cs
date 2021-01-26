using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dassanie.Models
{
    public class ClickSendResponse
    {
        public int http_code { get; set; }
        public string response_code { get; set; }
        public string response_msg { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public float total_price { get; set; }
        public int total_count { get; set; }
        public int queued_count { get; set; }
        public Message[] messages { get; set; }
        public _Currency _currency { get; set; }
    }

    public class _Currency
    {
        public string currency_name_short { get; set; }
        public string currency_prefix_d { get; set; }
        public string currency_prefix_c { get; set; }
        public string currency_name_long { get; set; }
    }

    public class Message
    {
        public string direction { get; set; }
        public int date { get; set; }
        public string to { get; set; }
        public string body { get; set; }
        public string from { get; set; }
        public int schedule { get; set; }
        public string message_id { get; set; }
        public int message_parts { get; set; }
        public string message_price { get; set; }
        public object from_email { get; set; }
        public object list_id { get; set; }
        public string custom_string { get; set; }
        public object contact_id { get; set; }
        public int user_id { get; set; }
        public int subaccount_id { get; set; }
        public string country { get; set; }
        public string carrier { get; set; }
        public string status { get; set; }
    }

}
