using Dassanie.Data;
using Dassanie.Helpers;
using LinqToTwitter;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dassanie.Controllers
{
    public class TweetController : Controller
    {
        private TweetController() { }
        private ApplicationDbContext _dbCtx;
        private TwitterContext _twtCtx;
        private MessageDeliveryHelper _messageHelper;
        public TweetController(ApplicationDbContext dbCtx)
        {
            _dbCtx = dbCtx;
            var auth = new MvcAuthorizer();
            _twtCtx = new TwitterContext(auth);
            _messageHelper = new MessageDeliveryHelper();
        }

        public async Task<IActionResult> Index()
        {
            SetUpTwitterContext();
            //This method is going to need to be rebuilt to scale up. At this point it's built with the belief
            //that the number of users/alerts will stay in the dozens-maybe-hundreds range


            var alertGroups = _dbCtx.Alerts.AsEnumerable().GroupBy(c => c.UserId);


            foreach(var userAlerts in alertGroups)
            {
                if (_dbCtx.UserDetails.Any(c => c.UserId == userAlerts.First().UserId))
                {
                    var userDetails = _dbCtx.UserDetails.First(c => c.UserId == userAlerts.First().UserId);
                    var userLogin = _dbCtx.UserLogins.First(c => c.LoginProvider == "Twitter" && c.UserId == userDetails.UserId);
                    var user = _dbCtx.Users.Find(userDetails.UserId);
                    _twtCtx.Authorizer.CredentialStore.OAuthToken = userDetails.TwitterOAuthToken;
                    _twtCtx.Authorizer.CredentialStore.OAuthTokenSecret = userDetails.TwitterOAuthSecret;
                    foreach(var alert in userAlerts)
                    {
                        var query = "from:" + alert.TwitterFollowId.ToString();
                        foreach (var word in alert.AlertWords)
                        {
                            query += " " + word;
                        }


                        var tweets = await _twtCtx.Status.Where(c => c.Type == StatusType.User && c.UserID == (ulong)alert.TwitterFollowId).ToListAsync();

                        if (tweets != null && tweets.Any(c => DateTime.Compare(alert.LastChecked, c.CreatedAt) <= 0))
                        {
                            var newTweets = tweets.Where(c => DateTime.Compare(alert.LastChecked, c.CreatedAt) <= 0).ToList();

                            foreach (var newTweet in newTweets)
                            {
                                if (alert.AlertWords.Any(c => newTweet.Text.Contains(c, StringComparison.CurrentCultureIgnoreCase)))
                                {
                                    _messageHelper.SendAlertsAsync(user, alert, newTweet);
                                }
                                
                            }
                        }  
                    }
                }

            }
            return View();
        }

        private void SetUpTwitterContext()
        {
            var session = HttpContext.Session;
            var apiKey = Environment.GetEnvironmentVariable("twitterApiKey");
            var apiSecret = Environment.GetEnvironmentVariable("twitterApiSecret");
            _twtCtx.Authorizer.CredentialStore = new SessionStateCredentialStore(session)
            {
                ConsumerKey = apiKey,
                ConsumerSecret = apiSecret
            };
        }
    }
}
