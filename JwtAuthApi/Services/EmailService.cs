using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Services.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


namespace JwtAuthApi.Interfaces
{

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _logger = logger;
            _config = config;
        }

        public async Task SendEmailAsync(EmailProps email)
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(
                    _config["Email:FromName"],
                    _config["Email:FromAddress"]
                ));

                emailMessage.To.Add(new MailboxAddress("", email.ToEmail));
                emailMessage.Subject = email.Subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = email.Body };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _config["Email:SmtpServer"],
                    int.Parse(_config["Email:Port"]!),
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    _config["Email:Username"],
                    _config["Email:Password"]
                );

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {email.ToEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
                throw;
            }
        }
        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var emailToSend = new EmailProps()
            {
                Subject = "Confirm Your Email Address",
                Body = await BodyConstructor("EmailConfirmation.html", confirmationLink),
                ToEmail = email
            };
            await SendEmailAsync(emailToSend);
        }
        public async Task Send2FACodeAsync(string email, string code)
        {
            var emailToSend = new EmailProps()
            {
                Subject = "Your Two-Factor Authentication Code",
                Body = await BodyConstructor("2FACode.html", code),
                ToEmail = email
            };

            await SendEmailAsync(emailToSend);
        }
        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var frontendUrl = _config["Frontend:Url"];
            var resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(email!)}&token={Uri.EscapeDataString(resetToken)}";
            var emailToSend = new EmailProps()
            {
                Subject = "Reset Your Password",
                Body = await BodyConstructor("ResetPasswordEmail.html", resetLink),
                ToEmail = email
            };

            await SendEmailAsync(emailToSend);
        }

        public async Task SendSellerApprovalEmail(string email)
        {
            var emailToSend = new EmailProps()
            {
                Subject = "Account Approval",
                Body = await BodyConstructor("EmailApproved.html", null),
                ToEmail = email
            };

            await SendEmailAsync(emailToSend);
        }

        public async Task SendSellerRejectionEmail(string email, string rejectionReason)
        {
            var emailToSend = new EmailProps()
            {
                Subject = "Account Approval",
                Body = await BodyConstructor("EmailReject.html", rejectionReason),
                ToEmail = email
            };

            await SendEmailAsync(emailToSend);
        }

        //HELPER
        private static async Task<string> BodyConstructor(string templateFile, string? link)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", templateFile);
            var body = await File.ReadAllTextAsync(templatePath);
            if (link != null)
            {
                body = body.Replace("{{replaceable}}", link);
            }
            return body;
        }


    }


}