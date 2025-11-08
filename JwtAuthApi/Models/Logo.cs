using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Models
{
    public class Logo
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public string PublicId { get; set; } = string.Empty; // Cloudinary public ID for deletion
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required]
        public string SellerId { get; set; } = string.Empty;
        public AppUser Seller { get; set; } = null!;
    }
}