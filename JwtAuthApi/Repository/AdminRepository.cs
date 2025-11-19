using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Admin;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Helpers.QueryBuilders;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class AdminRepository : IAdminRepository
    {

        private readonly IEmailService _emailService;
        private readonly UserManager<AppUser> _userManager;
        public AdminRepository(IEmailService emailService, UserManager<AppUser> userManager)
        {

            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task<OperationResult<object, string>> ApproveSellerAsync(SellerApprovalDto model, string adminId)
        {
            var seller = await _userManager.FindByIdAsync(model.SellerId);
            if (seller == null)
                return OperationResult<object, string>.Failure("Seller not found");
            if (seller.IsApproved)
                return OperationResult<object, string>.Failure("Seller is already approved");
            if (!model.Approve)
            {
                if (string.IsNullOrEmpty(model.RejectionReason))
                    return OperationResult<object, string>.Failure("Rejection reason is required!");
                seller.RejectionReason = model.RejectionReason;
                await _userManager.UpdateAsync(seller);

                await _emailService.SendSellerRejectionEmail(seller.Email!, model.RejectionReason);
                return OperationResult<object, string>.Success(new
                {
                    message = "Seller rejected",
                    businessName = seller.BusinessName,
                    reason = model.RejectionReason
                });
            }

            seller.IsApproved = true;
            seller.ApprovedAt = DateTime.UtcNow;
            seller.ApprovedBy = adminId;
            seller.RejectionReason = null;
            await _userManager.UpdateAsync(seller);

            await _emailService.SendSellerApprovalEmail(seller.Email!);
            return OperationResult<object, string>.Success(new
            {
                message = "Seller approved successfully",
                businessName = seller.BusinessName
            });
        }
        public async Task<PaginatedResponse<PendingSellerDto>> GetPendingSellersAsync(PendingSellerQueryObj queryObject)
        {
            var sellersQuery = _userManager.Users
                .Where(u => u.BusinessName != null && !u.IsApproved);

            sellersQuery = PendingSellerQueryBuilder.ApplyPendingSellerFilters(sellersQuery, queryObject);
            sellersQuery = PendingSellerQueryBuilder.ApplyPendingSellerSorting(sellersQuery, queryObject);

            // Pagination
            var skip = (queryObject.PageNumber - 1) * queryObject.PageSize;
            var totalCount = await sellersQuery.CountAsync();
            var sellers = await sellersQuery
                    .Skip(skip)
                    .Take(queryObject.PageSize)
                    .Select(u => u.UserToPendingSellerDto())
                    .ToListAsync();

            return new PaginatedResponse<PendingSellerDto>()
            {
                Total = totalCount,
                PageNumber = queryObject.PageNumber,
                PageSize = queryObject.PageSize,
                Items = sellers
            };
        }
    }
}