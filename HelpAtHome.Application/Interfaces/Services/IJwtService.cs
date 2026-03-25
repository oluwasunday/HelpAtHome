using HelpAtHome.Core.Entities;
using System.Security.Claims;

namespace HelpAtHome.Application.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(
            User user, IList<string> roles, IList<Claim> extraClaims);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }

}
