using Dassanie.Data;
using Dassanie.Models;
using LinqToTwitter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dassanie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowerAPI : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private IdentityUser _user;
        private TwitterContext _twtContext;
        private UserDetail _userDetails;

        public FollowerAPI(ApplicationDbContext context)
        {
            
            _context = context;
        }

        public IEnumerable<User> Get(string term)
        {
            List<User> users = new List<User>();
            if (!string.IsNullOrEmpty(term))
            {
                users = GetFollows(term);
            }
            return users;
        }

        private void SetupUser()
        {
            _user = _context.Users.First(c => c.Email == HttpContext.User.Identity.Name);
            _userDetails = _context.UserDetails.First(c => c.UserId == _user.Id);
        }

        private void SetupContext()
        {
            if (_userDetails == null)
            {
                _userDetails = _context.UserDetails.First(c => c.UserId == _user.Id);
            }

            _twtContext = new TwitterContext(new MvcAuthorizer()
            {
                CredentialStore = new SessionStateCredentialStore(HttpContext.Session)
                {
                    OAuthToken = _userDetails.TwitterOAuthToken,
                    OAuthTokenSecret = _userDetails.TwitterOAuthSecret,
                    ConsumerKey = Environment.GetEnvironmentVariable("twitterApiKey"),
                    ConsumerSecret = Environment.GetEnvironmentVariable("twitterApiSecret")
                }
            });
        }
        private List<User> GetFollows(string query)
        {
            SetupUser();
            SetupContext();
            long csr = -1;
            var follows = new List<User>();

            do
            {
                try
                {
                    var followerQuery = _twtContext.Friendship
                    .Where(c => c.Count == 500 && c.Cursor == csr && c.Type == FriendshipType.FriendsList
                            && c.UserID == _userDetails.TwitterId.ToString() && c.SkipStatus == true)
                    .SingleOrDefault();

                    if (followerQuery != null && followerQuery.Users.Any())
                    {
                        follows.AddRange(followerQuery.Users.Where(c=>Culture.US.CompareInfo.IndexOf(c.ScreenNameResponse, query, System.Globalization.CompareOptions.IgnoreCase)>=0).ToList());
                        csr = followerQuery.CursorMovement.Next;
                    }
                    else
                    {
                        csr = 0;
                    }
                }
                catch(Exception e)
                {
                    var i = 0;
                    i++;
                }
            }
            while (csr != 0);

            return follows;
        }
    }
}
