using Account_Track.Model;
using System.Security.Claims;

namespace Account_Track.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(t_User user);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

