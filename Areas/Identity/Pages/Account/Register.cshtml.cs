using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MimeKit;
using Swish.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Swish.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<SwishUser> _signInManager;
        private readonly UserManager<SwishUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<SwishUser> userManager,
            SignInManager<SwishUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "First name")]
            public string FName { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Last name")]
            public string LName { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Birth Date")]
            public DateTime DOB { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Profile Picture")]
            public string PPicPath { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Gender")]
            public string Gender { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                SwishUser user = new SwishUser
                {
                    UserName = Input.UserName,
                    FName = Input.FName,
                    LName = Input.LName,
                    DOB = Input.DOB,
                    PPicPath = "ProfilePlaceholder.png",
                    Gender = Input.Gender,
                    Email = Input.Email
                };
                try
                {
                    if (_userManager.FindByEmailAsync(Input.Email).Result.Email == Input.Email)
                    {
                        ViewData["Error"] = "User with given E-Mail already exists.";
                        return Page();
                    }
                }
                catch { }

                try
                {
                    if (_userManager.FindByNameAsync(Input.UserName).Result.UserName == Input.UserName)
                    {
                        ViewData["Error"] = "User with given Username already exists.";
                        return Page();
                    }
                }
                catch { }

                IdentityResult result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    string callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    MimeMessage message = new MimeMessage();
                    MailboxAddress from = new MailboxAddress("No-Reply", "noreply@swish.com");
                    BodyBuilder bodyBuilder = new BodyBuilder();
                    MailboxAddress to = new MailboxAddress(Input.UserName, Input.Email);
                    SmtpClient client = new SmtpClient();

                    bodyBuilder.HtmlBody = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";

                    message.From.Add(from);
                    message.To.Add(to);
                    message.Subject = "Account E-Mail confirmation";
                    message.Body = bodyBuilder.ToMessageBody();

                    client.Connect("mail.deepseagt.com", 465, true);
                    client.Authenticate("swish@deepseagt.com", "M^TQ*b#5Hfgb");
                    client.Send(message);
                    client.Disconnect(true);
                    client.Dispose();

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            ViewData["Error"] = "There was an error in registering";
            return Page();
        }
    }
}
