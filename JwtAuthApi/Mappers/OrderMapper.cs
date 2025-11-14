using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Orders;
using JwtAuthApi.Models;

namespace JwtAuthApi.Mappers
{
    public static class OrderMapper
    {
        public static OrderDto OrderToOrderDto(this Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                CustomerEmail = order.Customer.Email!,
                SellerId = order.SellerId,
                SellerName = order.Seller.BusinessName!,
                SellerEmail = order.Seller.Email!,
                SubTotal = order.SubTotal,
                DeliveryFee = order.DeliveryFee,
                Tax = order.Tax,
                Total = order.Total,
                Status = order.Status,
                DeliveryAddress = order.DeliveryAddress,
                PhoneNumber = order.PhoneNumber,
                Notes = order.Notes,
                EstimatedDeliveryTime = order.EstimatedDeliveryTime,
                CreatedAt = order.CreatedAt,
                ConfirmedAt = order.ConfirmedAt,
                PreparingAt = order.PreparingAt,
                ReadyAt = order.ReadyAt,
                DeliveredAt = order.DeliveredAt,
                CancelledAt = order.CancelledAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    FoodItemId = oi.FoodItemId,
                    FoodItemName = oi.FoodItem.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    SpecialInstructions = oi.SpecialInstructions,
                    LineTotal = oi.Quantity * oi.Price,
                    MainImageUrl = oi.FoodItem.ImageUrls.FirstOrDefault(img => img.IsMainImage)?.ImageUrl
                }).ToList()
            };
        }
    }
}