using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Foods
{
    public class UploadLogoDto
    {
        [Required]
        public required IFormFile File { get; set; }
    }
}