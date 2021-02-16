using Dassanie.Data;
using Dassanie.Helpers;
using LinqToTwitter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private ILogger<TweetController> _logger;
        public TweetController(ILogger<TweetController> logger, ApplicationDbContext dbCtx)
        {
            _dbCtx = dbCtx;
            _logger = logger;
            var auth = new MvcAuthorizer();
            _twtCtx = new TwitterContext(auth);
            _messageHelper = new MessageDeliveryHelper();
        }

        public async Task<IActionResult> Index()
        {
            
            _logger.LogInformation("The alert engine has been called.");
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
                        _logger.LogInformation($"We are checking tweets for the following word(s): {alert.TriggerWords}. This is for user {alert.UserId} and the twitter user {alert.TwitterFollowName} - {alert.TwitterFollowId}");
                        var query = "from:" + alert.TwitterFollowId.ToString() + " " + alert.TriggerWords.Trim();
                        

                        var tweets = await _twtCtx.Status.Where(c => c.Type == StatusType.User && c.UserID == (ulong)alert.TwitterFollowId).ToListAsync();

                        if (tweets != null && tweets.Any(c => DateTime.Compare(alert.LastChecked, c.CreatedAt) <= 0))
                        {
                            _logger.LogInformation($"There were new tweets from {alert.TwitterFollowName} since {alert.LastChecked}");
                            var newTweets = tweets.Where(c => DateTime.Compare(alert.LastChecked, c.CreatedAt) <= 0).ToList();

                            foreach (var newTweet in newTweets)
                            {
                                if (alert.AlertWords.Any(c => newTweet.Text.Contains(c, StringComparison.CurrentCultureIgnoreCase)))
                                {
                                    _logger.LogInformation($"We're sending {user.UserName} a tweet {newTweet.Text}");
                                    _messageHelper.SendAlertsAsync(user, alert, newTweet);
                                }
                                
                            }
                        }
                        alert.LastChecked = DateTime.UtcNow;
                        _dbCtx.Update(alert);
                    }
                    _dbCtx.SaveChanges();
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
