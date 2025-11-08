using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtAuthApi.Helpers.HelperObjects
{
    public class ErrorResult
    {
        public string ErrDescription { get; set; } = string.Empty;
        public int ErrCode { get; set; }
    }
}