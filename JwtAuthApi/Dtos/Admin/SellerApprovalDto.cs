using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Admin
{
    public class SellerApprovalDto
    {
        [Required]
        public string SellerId { get; set; } = string.Empty;

        [Required]
        public bool Approve { get; set; } // true = approve, false = reject

        public string? RejectionReason { get; set; } // Required if Approve = false
    }
}