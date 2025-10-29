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
            var body = $@"
                
                    Email Confirmation
                    Thank you for registering! Please confirm your email address by clicking the button below:
                    
                        
                            Confirm Email
                        
                    
                    
                        Or copy and paste this link into your browser:
                    
                    
                        {confirmationLink}
                    
                    
                        This link will expire in 24 hours. If you didn't create an account, please ignore this email.
                    
                
            ";

            await SendEmailAsync(email, subject, body);
        }
        public async Task Send2FACodeAsync(string email, string code)
        {
            var subject = "Your Two-Factor Authentication Code";
            var body = $@"
                
                    Two-Factor Authentication
                    Your verification code is:
                    
                        
                            {code}
                        
                    
                    
                        This code will expire in 5 minutes.
                    
                    
                        Enter this code in the application to complete your login.
                    
                    
                        If you didn't attempt to log in, please secure your account immediately by changing your password.
                    
                
            ";

            await SendEmailAsync(email, subject, body);
        }

    }


}