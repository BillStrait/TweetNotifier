using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dassanie.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dassanie.Models;
using LinqToTwitter;
using Microsoft.AspNetCore.Identity;

namespace Dassanie.Helpers.Tests
{
    [TestClass()]
    public class MessageDeliveryHelperTests
    {
        [TestMethod()]
        public async Task SendAlertSMSAsync()
        {
            var helper = new MessageDeliveryHelper();
            var user = new IdentityUser();
            user.PhoneNumber = "6122341115";
            user.PhoneNumberConfirmed = true;
            user.UserName = "Bill";
            var alert = new Alert();
            alert.SMS = true;
            var status = new Status();
            status.Text = "This is a pretend tweet";
            status.User = new User() { ScreenNameResponse = "Bob" };
            status.OEmbedUrl = "https://twitter.com/im_uname/status/1347999892392747011";


            var result = await helper.SendAlertsAsync(user, alert, status);
            Assert.IsTrue(result.SMSSuccess);
        }
    }
}