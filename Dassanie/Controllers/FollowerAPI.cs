using Dassanie.Data;
using Dassanie.Models;
using LinqToTwitter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        private IMemoryCache _cache;
        private readonly ApplicationDbContext _context;
        private IdentityUser _user;
        private TwitterContext _twtContext;
        private UserDetail _userDetails;

        public FollowerAPI(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            
            _context = context;
            _cache = memoryCache;
        }

        public IEnumerable<User> Get(string term)
        {
            List<User> users = new List<User>();
            if (!string.IsNullOrEmpty(term))
            {
                users = SearchFollows(term);
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
        private List<User> GetFollows()
        {
            var follows = new List<User>();

            if(!_cache.TryGetValue(User.Identity.Name + "-users", out follows))
            {
                follows = new List<User>();
                SetupUser();
                SetupContext();
                long csr = -1;

                do
                {
                    var followerQuery = _twtContext.Friendship
                    .Where(c => c.Cursor == csr && c.Type == FriendshipType.FriendsList && c.Count == 200
                            && c.UserID == _userDetails.TwitterId.ToString() && c.SkipStatus == true)
                    .SingleOrDefault();

                    if (followerQuery != null && followerQuery.Users.Any())
                    {
                        follows.AddRange(followerQuery.Users.ToList());
                        csr = followerQuery.CursorMovement.Next;
                    }
                    else
                    {
                        csr = 0;
                    }

                }
                while (csr != 0);
                var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                _cache.Set(User.Identity.Name + "-users", follows, options);
            }
            return follows;
        }

        private List<User> SearchFollows(string query)
        {
            var follows = GetFollows();

            return follows.Where(c => Culture.US.CompareInfo.IndexOf(c.ScreenNameResponse, query, System.Globalization.CompareOptions.IgnoreCase) >= 0).ToList();
        }
    }
}
