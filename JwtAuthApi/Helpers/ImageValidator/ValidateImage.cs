using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Helpers.ImageValidator
{
    public class ValidateImage
    {
        public static bool IsValidImage(IFormFile logo)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return false;

            // Validate file size (5MB)
            if (logo.Length > 5 * 1024 * 1024)
                return false;
            return true;
        }
    }
}