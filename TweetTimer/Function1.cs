using System;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace TweetTimer
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var request = HttpWebRequest.Create("https://twitteralerts.azurewebsites.net/Tweet");
            var result = request.GetResponse();
            //we don't care about the result, we only want to confirm it executes.
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
