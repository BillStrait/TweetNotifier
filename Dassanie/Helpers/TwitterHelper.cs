using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dassanie.Data;
using LinqToTwitter;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace Dassanie.Helpers
{
    public class TwitterHelper
    {
        private TwitterContext _ctx { get; set; }
        private ApplicationDbContext _dbCtx { get; set; }
        private TwitterHelper()
        {
        }

        public TwitterHelper(ApplicationDbContext dbContext, ISession session, int alertId)
        {
            _dbCtx = dbContext;
            //TODO: pull the alert and all the needed bits from the db.

            //TODO replace this code with something that gets the appropriate twitter context.
            var userDetails = _dbCtx.UserDetails.Find(1);
            var userLogin = _dbCtx.UserLogins.First(c => c.LoginProvider == "Twitter" && c.UserId == userDetails.UserId);
            ulong pKey = 0;
            ulong.TryParse(userLogin.ProviderKey, out pKey);
            var auth = new MvcAuthorizer()
            {
                CredentialStore = new SessionStateCredentialStore(session)
                {
                    OAuthToken = userDetails.TwitterOAuthToken,
                    OAuthTokenSecret = userDetails.TwitterOAuthSecret,
                    ScreenName = userDetails.DisplayName,
                    UserID = pKey
                }
            };
            _ctx = new TwitterContext(auth);
        }

        public void GetTweet()
        {
            var tweet = _ctx.Tweets.First();
            var i = 0;
            i++;
        }



    }
}
