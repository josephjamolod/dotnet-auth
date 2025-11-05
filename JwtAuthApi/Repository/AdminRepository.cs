using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Admin;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;
using Microsoft.AspNetCore.Identity;

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

                // TODO: Send rejection email
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
            // TODO: Send approval email
            return OperationResult<object, string>.Success(new
            {
                message = "Seller approved successfully",
                businessName = seller.BusinessName
            });
        }
    }
}