using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.User
{
    public class DeleteAccountDto
    {
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string ConfirmationText { get; set; } = string.Empty;
    }
}