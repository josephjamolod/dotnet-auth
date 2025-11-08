using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JwtAuthApi.Repository.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SortByOption
    {
        Name,
        LastName
    }
    public class PendingSellerQueryObj
    {
        public string? FirstName { get; set; } = null;
        public string? LastName { get; set; } = null;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public SortByOption? SortBy { get; set; } = null;
        public bool IsDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}