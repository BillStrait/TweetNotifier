using Dassanie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;
using Microsoft.AspNetCore.Identity;
using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.ClickSend.Model;
using IO.ClickSend.Client;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace Dassanie.Helpers
{
    public class MessageDeliveryHelper
    {
        private ILogger _logger;

        public MessageDeliveryHelper()
        {
            //TODO: Add a moq logger.
        }

        public MessageDeliveryHelper(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<AlertResult> SendAlertsAsync(IdentityUser user, Alert alert, Status tweet)
        {
            Task<bool> emailSuccessTask = null, smsSuccessTask = null;
            bool? emailSuccess = null, smsSuccess = null;
            var result = new AlertResult();

            if (alert.Email && user.EmailConfirmed)
            {
                var message = $"{tweet.User.ScreenNameResponse} tweeted:\n\n{tweet.Text}";
                if (alert.IncludeLink)
                {
                    message += $"\n\n{tweet.User.ScreenNameResponse}/status/{tweet.StatusID}";
                }
                //emailSuccessTask = SendEmailAsync(user.Email, message);
            }

            if (alert.SMS && user.PhoneNumberConfirmed)
            {
                var message = $"{tweet.User.ScreenNameResponse} tweeted:\n\n{tweet.Text}";
                if (alert.IncludeLink)
                {
                    message += $"\n\n{tweet.User.ScreenNameResponse}/status/{tweet.StatusID}";
                }
                if (_logger != null)
                {
                    _logger.LogInformation($"Sending an SMS to {alert.UserId}");
                }
                
                smsSuccessTask = SendSMSAsync(user.PhoneNumber, message);
            }

            if(emailSuccessTask != null)
            {
                result.EmailSuccess = await emailSuccessTask;
            }

            if(smsSuccessTask != null)
            {
                result.SMSSuccess = await smsSuccessTask;
            }

            return result;
        }
        public async Task<bool> ConfirmSMS(IdentityUser user)
        {
            var message = "Someone, hopefully you, signed up for a twitter alert service with this number. If this was not you, no action is needed. If it is, please reply with: yes";
            var smsSuccessTask = SendSMSAsync(user.PhoneNumber, message);
            return await smsSuccessTask;

        }

        private async Task<bool> SendSMSAsync(string number, string message)
        {

            var userName = Environment.GetEnvironmentVariable("clickSendUser");
            var apiKey = Environment.GetEnvironmentVariable("clickSendApiKey");
            var config = new IO.ClickSend.Client.Configuration();
            config.Username = userName;
            config.Password = apiKey;
            var clickSend = new SMSApi(config);
            var smsMessage = new SmsMessage(null,message,number);

            var sms = new List<SmsMessage>()
            {
                smsMessage
            };

            var collection = new SmsMessageCollection(sms);
            var responseTask = clickSend.SmsSendPostAsync(collection);

            var responseString = await responseTask;
            ClickSendResponse response = JsonSerializer.Deserialize<ClickSendResponse>(responseString);
            
            if (response.http_code == 200)
            {
                //TODO: there is a 'data' object in the deserializer that lets us know if the text went through
                if (_logger != null)
                {
                    _logger.LogInformation($"We got a positive message back from Clicksend. They say the satus of the message is: {response.data.messages[0].status}");
                }
                
                return true;
            }
            if (_logger != null)
            {
                _logger.LogError($"Our attempt to send an sms message failed. Here's what we know. \nHttp response code: {response.http_code} \nResponse message:{response.response_msg}");
            }
            return false;

        }
    }
}
