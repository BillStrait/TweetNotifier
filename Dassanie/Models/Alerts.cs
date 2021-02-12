using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dassanie.Models
{
    public class Alert
    {
        public Alert() { }
        public Alert(AlertVM vm)
        {
            AlertId = vm.AlertId;
            UserId = vm.UserId;
            TwitterFollowId = vm.SelectedFollower;
            Twitter = vm.Twitter;
            Facebook = vm.Facebook;
            WhatsApp = vm.WhatsApp;
            Email = vm.Email;
            IncludeLink = vm.IncludeLink;
            AlertWords = vm.AlertWords.Split(' ').ToList();
        }

        public int AlertId { get; set; }
        public string UserId { get; set; }
        public int TwitterFollowId { get; set; }
        public string TwitterFollowName { get; set; }
        public bool SMS { get; set; }
        public bool Twitter { get; set; }
        public bool Facebook { get; set; }
        public bool WhatsApp { get; set; }
        public bool Email { get; set; }
        public bool IncludeLink { get; set; }
        public string TriggerWords { get; set; }
        [NotMapped]
        public List<string> AlertWords
        {
            get
            {
                return this.TriggerWords.Split(' ').ToList();
            }
            set
            {
                foreach(var word in value)
                {
                    this.TriggerWords += word + ' ';
                }
            }
        }
        public DateTime LastChecked { get; set; }
    }

    public class AlertResult
    {
        public bool SMSSuccess { get; set; }
        public bool TwitterSuccess { get; set; }
        public bool FacebookSuccess { get; set; }
        public bool WhatsAppSuccess { get; set; }
        public bool EmailSuccess { get; set; }
        public string Message { get; set; }
    }

    public class AlertVM
    {
        public AlertVM() { }
        public AlertVM(Alert alert)
        {
            AlertId = alert.AlertId;
            UserId = alert.UserId;
            Twitter = alert.Twitter;
            Facebook = alert.Facebook;
            WhatsApp = alert.WhatsApp;
            Email = alert.Email;
            IncludeLink = alert.IncludeLink;
            SelectedFollower = alert.TwitterFollowId;
            FollowerName = alert.TwitterFollowName;
            AlertWords = alert.TriggerWords;
            
        }

        public int AlertId { get; set; }
        public string UserId { get; set; }
        public bool SMS { get; set; }
        public bool Twitter { get; set; }
        public bool Facebook { get; set; }
        public bool WhatsApp { get; set; }
        public bool Email { get; set; }
        public bool IncludeLink { get; set; }
        public string AlertWords { get; set; }
        public string FollowerName { get; set; }
        public int SelectedFollower { get; set; }
    }
}
