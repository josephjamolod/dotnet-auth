using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Admin;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<AppUser> _userManager;
        public AdminRepository(ApplicationDBContext context, IEmailService emailService, UserManager<AppUser> userManager)
        {
            _context = context;
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
        public async Task<object> GetPendingSellersAsync(PendingSellerQueryObj queryObject)
        {
            var sellersQuery = _userManager.Users
                .Where(u => u.BusinessName != null && !u.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrEmpty(queryObject.FirstName))
                sellersQuery = sellersQuery.Where(u => u.FirstName != null && u.FirstName.Contains(queryObject.FirstName));

            if (!string.IsNullOrEmpty(queryObject.LastName))
                sellersQuery = sellersQuery.Where(u => u.LastName != null && u.LastName.Contains(queryObject.LastName));

            if (!string.IsNullOrEmpty(queryObject.Email))
                sellersQuery = sellersQuery.Where(u => u.Email!.Contains(queryObject.Email));

            if (!string.IsNullOrEmpty(queryObject.PhoneNumber))
                sellersQuery = sellersQuery.Where(u => u.PhoneNumber!.Contains(queryObject.PhoneNumber));

            // Sorting
            switch (queryObject.SortBy)
            {
                case SortByOption.Name:
                    if (queryObject.IsDescending)
                        sellersQuery = sellersQuery.OrderByDescending(u => u.FirstName);
                    else
                        sellersQuery = sellersQuery.OrderBy(u => u.FirstName);
                    break;

                case SortByOption.LastName:
                    if (queryObject.IsDescending)
                        sellersQuery = sellersQuery.OrderByDescending(u => u.LastName);
                    else
                        sellersQuery = sellersQuery.OrderBy(u => u.LastName);
                    break;

                default:
                    sellersQuery = sellersQuery.OrderByDescending(u => u.CreatedAt);
                    break;
            }
            // Pagination
            var skip = (queryObject.PageNumber - 1) * queryObject.PageSize;
            var totalCount = await sellersQuery.CountAsync();
            var sellers = await sellersQuery
                    .Skip(skip)
                    .Take(queryObject.PageSize)
                    .Select(u => u.UserToPendingSellerDto())
                    .ToListAsync();

            return new
            {
                total = totalCount,
                pageNumber = queryObject.PageNumber,
                pageSize = queryObject.PageSize,
                sellers
            };
        }
    }
}