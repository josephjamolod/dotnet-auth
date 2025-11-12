using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Cart
{
    public class CartValidationResult
    {
        public bool IsValid { get; set; } = true;
        public bool CanProceed { get; set; } = true;  // False if has unavailable items
        public bool RequiresConfirmation { get; set; } = false; // True if has price changes
        public bool HasPriceChanges { get; set; }
        public bool HasUnavailableItems { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new();
    }
}