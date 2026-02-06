using Account_Track.DTOs.AuthDto;

namespace Account_Track.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginRequestDto dto);

        Task<LoginResponseDto> RefreshToken(RefreshTokenRequestDto dto);

        Task<string> Logout(int userId);
    }
}
