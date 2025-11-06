using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Models;

namespace JwtAuthApi.Dtos.Seller
{
    public class UpdateSellerProfileDto
    {
        [Required]
        [StringLength(30)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        public void UpdateSellerProfileDtoToSeller(AppUser seller)
        {
            seller.FirstName = this.FirstName ?? seller.FirstName;
            seller.LastName = this.LastName ?? seller.LastName;
            seller.PhoneNumber = this.PhoneNumber ?? seller.PhoneNumber;
            seller.BusinessName = this.BusinessName ?? seller.BusinessName;
            seller.Description = this.Description ?? seller.Description;
            seller.Address = this.Address ?? seller.Address;
            seller.City = this.City ?? seller.City;
            seller.State = this.State ?? seller.State;
            seller.PostalCode = this.PostalCode ?? seller.PostalCode;
            seller.UpdatedAt = DateTime.UtcNow;
        }
    }
}