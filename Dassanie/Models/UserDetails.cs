using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dassanie.Models
{
    public class UserDetail
    {
        public UserDetail() { }

        public UserDetail(IdentityUser user)
        {
            UserId = user.Id;
            DisplayName = user.UserName;
            Email = user.Email;
        }

        public UserDetail(IdentityUser user, AuthenticationProperties props)
        {
            UserId = user.Id;
            DisplayName = user.UserName;
            Email = user.Email;
            TwitterOAuthToken = props.GetTokenValue("access_token");
            TwitterOAuthSecret = props.GetTokenValue("access_token_secret");
            return;
        }

        public int UserDetailId { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public ulong TwitterId { get; set; }
        public string TwitterOAuthToken { get; set; }
        public string TwitterOAuthSecret { get; set; }
        public string MobileNumber { get; set; }
        public string FacebookId { get; set; }
        public string WhatsAppId { get; set; }
        public string Email { get; set; }
    }
}
