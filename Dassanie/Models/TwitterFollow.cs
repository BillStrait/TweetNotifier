using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dassanie.Models
{
    public class TwitterFollow
    {
        public int TwitterFollowId { get; set; }
        public int TwitterId { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; }
    }
}
