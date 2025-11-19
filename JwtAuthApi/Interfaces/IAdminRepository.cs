using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Admin;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Helpers.HelperObjects;


namespace JwtAuthApi.Interfaces
{
    public interface IAdminRepository
    {
        Task<OperationResult<object, string>> ApproveSellerAsync(SellerApprovalDto model, string adminId);
        Task<PaginatedResponse<PendingSellerDto>> GetPendingSellersAsync(PendingSellerQueryObj queryObject);
    }
}