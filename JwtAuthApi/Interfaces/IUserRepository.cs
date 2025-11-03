using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.User;
using JwtAuthApi.Repository.Models;

namespace JwtAuthApi.Interfaces
{
    public interface IUserRepository
    {
        Task<OperationResult<UserProfileDto, ErrorResult>> GetUserProfileAsync(string userId);
    }
}