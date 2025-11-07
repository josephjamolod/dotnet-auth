using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Dtos.Foods
{
    public class FoodImageResponseDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? PublicId { get; set; }
        public bool IsMain { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}