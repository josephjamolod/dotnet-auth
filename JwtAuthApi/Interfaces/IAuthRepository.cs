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
        Task<OperationResult<AppUser, string>> ResendEmailConfirmationAsync(ResendConfirmationDto model);
    }
}