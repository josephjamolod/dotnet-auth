using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Admin;
using JwtAuthApi.Repository.Models;

namespace JwtAuthApi.Interfaces
{
    public interface IAdminRepository
    {
        Task<OperationResult<object, string>> ApproveSellerAsync(SellerApprovalDto model, string adminId);
    }
}