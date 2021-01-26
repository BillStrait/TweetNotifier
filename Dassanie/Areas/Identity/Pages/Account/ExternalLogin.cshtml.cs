using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Dassanie.Data;
using Dassanie.Helpers;
using Dassanie.Models;
using LinqToTwitter;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Dassanie.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private ApplicationDbContext _dbctx;
        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _dbctx = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [Phone]
            [Display(Name="SMS Number")]
            public string PhoneNumber { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/Alerts/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var props = new AuthenticationProperties();
                props.StoreTokens(info.AuthenticationTokens);

                UserDetail userDetails;

                if (_dbctx.UserDetails.Any(c => c.UserId == user.Id)) {
                    userDetails = _dbctx.UserDetails.First(c => c.UserId == user.Id);
                    userDetails.TwitterOAuthToken = props.GetTokenValue("access_token");
                    userDetails.TwitterOAuthSecret = props.GetTokenValue("access_token_secret");
                    _dbctx.UserDetails.Update(userDetails);
                }
                else 
                {
                    userDetails = new UserDetail(user, props);
                    ulong twitterId;
                    ulong.TryParse(info.Principal.Identities.First().Claims.First(c => c.Type == "urn:twitter:userid").Value, out twitterId);
                    userDetails.TwitterId = twitterId;
                    _dbctx.UserDetails.Add(userDetails);
                }
                
                
                
                var saveTask = await _dbctx.SaveChangesAsync();

                await _signInManager.SignInAsync(user, props, info.LoginProvider);

                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                    if (info.Principal.HasClaim(c => c.Type == ClaimTypes.MobilePhone))
                    {
                        Input.PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone);
                    }
                    else if (info.Principal.HasClaim(c => c.Type == ClaimTypes.HomePhone)) 
                    {
                        Input.PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.HomePhone);
                    }
                    
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email, PhoneNumber = Input.PhoneNumber };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        var helper = new MessageDeliveryHelper();
                        await helper.ConfirmSMS(user);

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        //we want to store these credentials.
                        var props = new AuthenticationProperties();
                        props.StoreTokens(info.AuthenticationTokens);
                        props.IsPersistent = true;

                        UserDetail userDetails;

                        if (_dbctx.UserDetails.Any(c => c.UserId == user.Id))
                        {
                            userDetails = _dbctx.UserDetails.First(c => c.UserId == user.Id);
                            userDetails.TwitterOAuthToken = props.GetTokenValue("access_token");
                            userDetails.TwitterOAuthSecret = props.GetTokenValue("access_token_secret");
                            _dbctx.UserDetails.Update(userDetails);
                        }
                        else
                        {
                            userDetails = new UserDetail(user, props);
                            _dbctx.UserDetails.Add(userDetails);
                        }



                        var saveTask = await _dbctx.SaveChangesAsync();

                        await _signInManager.SignInAsync(user, props);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
