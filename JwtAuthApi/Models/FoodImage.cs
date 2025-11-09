using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Models
{
    public class FoodImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public string PublicId { get; set; } = string.Empty; // Cloudinary public ID for deletion

        public bool IsMainImage { get; set; } = false; // Flag for primary image

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required]
        public int FoodItemId { get; set; }

        public FoodItem FoodItem { get; set; } = null!;
    }
}
