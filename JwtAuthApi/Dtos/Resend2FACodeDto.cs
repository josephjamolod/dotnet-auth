using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos
{
    public class Resend2FACodeDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }
}