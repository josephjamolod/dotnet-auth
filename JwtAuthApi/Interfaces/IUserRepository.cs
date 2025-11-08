using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.User;
using JwtAuthApi.Helpers.HelperObjects;


namespace JwtAuthApi.Interfaces
{
    public interface IUserRepository
    {
        Task<OperationResult<UserProfileDto, ErrorResult>> GetUserProfileAsync(string userId);
        Task<OperationResult<object, ErrorResult>> UpdateProfileAsync(string userId, UpdateProfileDto model);
        Task<OperationResult<object, ErrorResult>> DeleteAccountAsync(string userId, DeleteAccountDto model);
    }
}