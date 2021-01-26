using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.ClickSend.Model;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dassanie.Services
{
    public class EmailSender : IEmailSender
    {
        public AuthMessageSenderOptions Options { get; }
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(subject, message, email);
        }

        public Task Execute(string subject, string message, string email)
        {
            var config = new IO.ClickSend.Client.Configuration();
            config.Username = Options.ClickSendUser;
            config.Password = Options.ClickSendApiKey;

            var transactionalEmailApi = new TransactionalEmailApi(config);
            var listOfRecipients = new List<EmailRecipient>();
            listOfRecipients.Add(new EmailRecipient(
                email: email,
                name: email
            ));
            
            var emailFrom = new EmailFrom(
                emailAddressId: Options.EmailAddressId.ToString(),
                name: "Do Not Reply"
            );

            //response will contain diagnostic information if email confirmation debugging is needed. Probably best to
            //look at the clicksend dashboard first.
            var response = transactionalEmailApi.EmailSendPostAsync(new Email(
                to: listOfRecipients,
                cc: null,
                bcc: null,
                from: emailFrom,
                subject: subject,
                body: message,
                attachments: null
            ));

            return response;
        }
    }

    public class AuthMessageSenderOptions
    {
        public string ClickSendUser { get; set; }
        public string ClickSendApiKey { get; set; }
        public int EmailAddressId { get; set; }
    }
}
