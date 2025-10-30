using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(
                    _config["Email:FromName"],
                    _config["Email:FromAddress"]
                ));

                emailMessage.To.Add(new MailboxAddress("", toEmail));
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };
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

                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
                throw;
            }
        }
        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Confirm Your Email Address";
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "EmailConfirmation.html");
            var body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{ConfirmationLink}}", confirmationLink);

            await SendEmailAsync(email, subject, body);
        }
        public async Task Send2FACodeAsync(string email, string code)
        {
            var subject = "Your Two-Factor Authentication Code";
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "2FACode.html");
            var body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{code}}", code);

            await SendEmailAsync(email, subject, body);
        }
        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "Reset Your Password";
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "ResetEmail.html");
            var body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{resetLink}}", resetLink);

            await SendEmailAsync(email, subject, body);
        }
    }


}