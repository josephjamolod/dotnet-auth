using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Orders;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Models;

namespace JwtAuthApi.Mappers
{
    public static class SellerMappers
    {
        public static PendingSellerDto UserToPendingSellerDto(this AppUser user)
        {
            return new PendingSellerDto()
            {
                Id = user.Id,
                BusinessName = user.BusinessName,
                BusinessNumber = user.BusinessNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = $"{user.Address}, {user.City}, {user.State} {user.PostalCode}",
                Description = user.Description,
                RegisteredAt = user.CreatedAt
            };
        }
        public static SellerProfileDto UserToSellerProfileDto(this AppUser seller)
        {
            return new SellerProfileDto()
            {
                // Personal info
                FirstName = seller.FirstName,
                LastName = seller.LastName,
                Email = seller.Email,
                PhoneNumber = seller.PhoneNumber,

                // Business info
                BusinessName = seller.BusinessName,
                BusinessNumber = seller.BusinessNumber,
                Description = seller.Description,
                Logo = seller.Logo?.ImageUrl,

                // Address
                Address = seller.Address,
                City = seller.City,
                State = seller.State,
                PostalCode = seller.PostalCode,

                // Status
                IsApproved = seller.IsApproved,
                IsActive = seller.IsActive,
                Rating = seller.Rating,
                TotalRatings = seller.TotalRatings,


                // Timestamps
                CreatedAt = seller.CreatedAt,
                ApprovedAt = seller.ApprovedAt
            };
        }

        public static Order CreateOrderDtoToOrder(this CreateOrderDto model)
        {
            var deliveryFeeValue = model.DeliveryFee ?? 0;
            var tax = model.SubTotal * 0.12m;
            var total = model.SubTotal + deliveryFeeValue + tax;
            return new Order()
            {
                CustomerId = model.UserId,
                SellerId = model.SellerId,
                SubTotal = model.SubTotal,
                DeliveryFee = deliveryFeeValue,
                Tax = model.SubTotal * 0.12m,
                Total = total,
                DeliveryAddress = model.DeliveryAddress,
                PhoneNumber = model.PhoneNumber,
                Notes = model.Notes,
                EstimatedDeliveryTime = model.EstimatedDeliveryTime,
                Status = OrderStatus.Pending
            };
        }
    }

}