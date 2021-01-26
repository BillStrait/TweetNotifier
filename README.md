# TweetNotifier

This application is built using ASP.NET Core, and should be able to run in any enviornment that supports it. 

## Configuration

You will need the following enviornmental variables set:

clickSendApiKey - the API key for your ClickSend account
ClickSendUser - The user id tied to your ClickSend account
emailAddressId - The email address id you created in ClickSend
twitterApiKey - Your Twitter API Key
twitterApiSecret - Your Twitter API Secret


## Deployment
I have deployed this using the following resources in Azure.

Resource Group - All in one for easy management
  * App Service Plan - I used B1 Linux
  * App Service
  * Function App - This doesn't have to be huge; it's just calling a URL and disregarding the results every 5 minutes. 
  * SQL Server - Scale to your needs. This application is not built to check thousands of alerts at once.
  * SQL Database
  
## Alternative Deployment thoughts:
Anything smart enough to run .NET Core, NuGet, and Entity Framework migrations would do the trick. I suspect you can build a Linux/Apache/MySql stack that could pull it off.
The easiest part would be replacing the Function App with a chron job. If you have a resource that's always online anyway, that would be the ideal way to call the function.
  
## Authentication
This application is currently configured to accept Twitter logins only. Other services can be added following [this](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-5.0) documentation.
You will need to be mindful that the Twitter queries are dependent on valid Twitter tokens per user; so I would suggest you add other accounts after registration.

Any user with a valid email and twitter account can register on this site. Functionality would need to be added to allow an administrator account to limit access. As of right now
there is no concept of roles in this application. It can be added fairly easily following the documentation [here](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-5.0).

## Scaling thoughts
This application is not intended to work at scale. You will want to take a close look at TweetController.cs if performance begins to drag. 
It would not be difficult to break all of the API-like functionality into microservices. It was built this way intentionally for the current goals of the project, but I hope 
I've left it clear and modular enough for others to pick up easily. 
