using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Data;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Interfaces;
using JwtAuthApi.Mappers;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Repository
{
    public class SellerRepository : ISellerRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDBContext _context;
        public SellerRepository(UserManager<AppUser> userManager, ApplicationDBContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<OperationResult<SellerProfileDto, string>> GetSellerProfileAsync(string sellerId)
        {
            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller == null)
                return OperationResult<SellerProfileDto, string>.Failure("Seller Not Found");

            var totalOrder = await _context.Orders.Where(o => o.SellerId == seller.Id).CountAsync();
            var totalItems = await _context.FoodItems.Where(f => f.SellerId == seller.Id).CountAsync();

            var sellerProfile = seller.UserToSellerProfileDto();
            sellerProfile.TotalItems = totalItems;
            sellerProfile.TotalOrders = totalOrder;

            return OperationResult<SellerProfileDto, string>.Success(sellerProfile);
        }
    }
}