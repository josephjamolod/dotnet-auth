using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Dtos.Seller;
using JwtAuthApi.Repository.Models;

namespace JwtAuthApi.Interfaces
{
    public interface ISellerRepository
    {
        Task<OperationResult<SellerProfileDto, string>> GetSellerProfileAsync(string sellerId);
        Task<OperationResult<object, ErrorResult>> UpdateSellerAsync(UpdateSellerProfileDto model, string sellerId);
        Task<OperationResult<object, string>> ToggleStatusAsync(string sellerId);
    }
}