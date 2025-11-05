using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Services.Models;

namespace JwtAuthApi.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailProps email);
        Task SendEmailConfirmationAsync(string email, string confirmationLink);

        Task Send2FACodeAsync(string email, string code);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendSellerApprovalEmail(string email);
        Task SendSellerRejectionEmail(string email, string rejectionReason);
    }
}