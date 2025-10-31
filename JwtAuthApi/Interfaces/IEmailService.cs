using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendEmailConfirmationAsync(string email, string confirmationLink);

        Task Send2FACodeAsync(string email, string code);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
    }
}