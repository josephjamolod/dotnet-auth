using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Models;

namespace JwtAuthApi.Mappers
{
    public static class SellerMappers
    {
        public static PendingSellerDto UserToSellerDto(this AppUser user)
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
    }
}