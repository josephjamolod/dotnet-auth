using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Models;

namespace JwtAuthApi.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(AppUser user, IList<string> roles);
        RefreshToken GenerateRefreshToken(string ipAddress);
    }
}