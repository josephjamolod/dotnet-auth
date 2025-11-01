using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;

namespace JwtAuthApi.Interfaces
{
    public interface IAuthRepository
    {
        Task<OperationResult<AppUser, string>> CreateUserAsync(RegisterDto model);
        Task<OperationResult<object, string>> ConfirmEmailAsync(ConfirmEmailDto model);
        Task<OperationResult<AppUser?, string>> ResendEmailConfirmationAsync(ResendConfirmationDto model);
        Task<OperationResult<AppUser, string>> LoginAsync(LoginDto model);
        Task<OperationResult<AppUser, string>> Verify2FAAsync(string username, string code);
        Task<OperationResult<AppUser?, string>> Resend2FACodeAsync(string username);
        Task<OperationResult<AppUser?, string>> Enable2FAAsync(string userId);
        Task<OperationResult<AppUser?, string>> Disable2FAAsync(string userId);
    }
}