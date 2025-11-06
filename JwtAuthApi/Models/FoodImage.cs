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
        public string ImageUrl { get; set; } = string.Empty;

        //Foreign Key
        public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; } = null!;
    }
}