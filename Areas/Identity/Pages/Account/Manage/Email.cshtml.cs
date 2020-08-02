﻿using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;
using Swish.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Swish.Areas.Identity.Pages.Account.Manage
{
    public partial class EmailModel : PageModel
    {
        private readonly UserManager<SwishUser> _userManager;
        private readonly SignInManager<SwishUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<SwishUser> userManager,
            SignInManager<SwishUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public string Username { get; set; }

        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(SwishUser user)
        {
            string email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            SwishUser user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            SwishUser user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            string email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                string userId = await _userManager.GetUserIdAsync(user);
                string code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                string callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);

                MimeMessage message = new MimeMessage();
                MailboxAddress from = new MailboxAddress("No-Reply", "noreply@swish.com");
                BodyBuilder bodyBuilder = new BodyBuilder();
                MailboxAddress to = new MailboxAddress(Input.NewEmail);
                SmtpClient client = new SmtpClient();

                bodyBuilder.HtmlBody = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";

                message.From.Add(from);
                message.To.Add(to);
                message.Subject = "Confirm your email";
                message.Body = bodyBuilder.ToMessageBody();

                client.Connect("mail.deepseagt.com", 465, true);
                client.Authenticate("swish@deepseagt.com", "M^TQ*b#5Hfgb");
                client.Send(message);
                client.Disconnect(true);
                client.Dispose();

                StatusMessage = "Confirmation link to change email sent. Please check your email.";
                return RedirectToPage();
            }

            StatusMessage = "Your email is unchanged.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            SwishUser user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            string email = await _userManager.GetEmailAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            MimeMessage message = new MimeMessage();
            MailboxAddress from = new MailboxAddress("No-Reply", "noreply@swish.com");
            BodyBuilder bodyBuilder = new BodyBuilder();
            MailboxAddress to = new MailboxAddress(Input.NewEmail);
            SmtpClient client = new SmtpClient();

            bodyBuilder.HtmlBody = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";

            message.From.Add(from);
            message.To.Add(to);
            message.Subject = "Confirm your email";
            message.Body = bodyBuilder.ToMessageBody();

            client.Connect("mail.deepseagt.com", 465, true);
            client.Authenticate("swish@deepseagt.com", "M^TQ*b#5Hfgb");
            client.Send(message);
            client.Disconnect(true);
            client.Dispose();

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}