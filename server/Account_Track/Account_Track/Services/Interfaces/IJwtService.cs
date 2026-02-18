using Account_Track.DTOs.AuthDto;
using System.Security.Claims;

namespace Account_Track.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(FindUserDto user);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

