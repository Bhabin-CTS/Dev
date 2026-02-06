using Account_Track.Data;
using Account_Track.DTOs.AuthDto;
using Account_Track.Utils;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Account_Track.Utils.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Account_Track.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext context, IJwtService jwtService, IConfiguration config)
        {
            _context = context;
            _jwtService = jwtService;
            _config = config;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto dto)
        {
            var sql = "EXEC USP_GetUserByEmail @Email"; 
            var parameters = new[] 
            { 
                new SqlParameter("@Email", dto.Email) 
            };
            var users = await _context.Database
                .SqlQueryRaw<FindUserDto>(sql, parameters)
                .ToListAsync();

            var user = users.FirstOrDefault();

            if (user == null)
                throw new BusinessException("USER_NOT_FOUND","User not found");

            //CHECK IF ACCOUNT LOCKED
            if (user.IsLocked)
                throw new BusinessException("ACCOUNT_LOCKED","User account is locked due to multiple failed attempts");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                // Update failed attempts
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC usp_UpdateUserFailedAttempt @UserId",
                    new SqlParameter("@UserId", user.UserId)
                );

                throw new BusinessException("INVALID_CREDENTIALS","Invalid credentials");
            }

            if (user.Status != UserStatus.Active)
                throw new BusinessException("USER_INACTIVE","User is not active");


            // GENERATE TOKENS
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshDays = Convert.ToDouble(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);


            // Store refresh token in LoginLog
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_InsertLoginLog @UserId, @RefreshToken, @RefreshTokenExpiry",
                new SqlParameter("@UserId", user.UserId),
                new SqlParameter("@RefreshToken", refreshToken),
                new SqlParameter("@RefreshTokenExpiry", refreshExpiry)
            );

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_ResetUserAttempts @UserId",
                new SqlParameter("@UserId", user.UserId)
            );

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<LoginResponseDto> RefreshToken(RefreshTokenRequestDto dto)
        {
            var principal =
                _jwtService.GetPrincipalFromExpiredToken(dto.AccessToken);

            var userId =
                int.Parse(principal.FindFirst("UserId").Value);
            
            var sqlUser = "EXEC usp_GetUserById @UserId"; 
            var userParams = new[] { 
                new SqlParameter("@UserId", userId) 
            }; 
            var users = await _context.Database
                .SqlQueryRaw<FindUserDto>(sqlUser, userParams)
                .ToListAsync();
            var user = users.FirstOrDefault();

            if (user == null)
                throw new BusinessException("INVALID_USER","Invalid user");

            var sqlLog = "EXEC usp_GetValidLoginLog @UserId, @RefreshToken"; 
            var logParams = new[] 
            { 
                new SqlParameter("@UserId", userId), 
                new SqlParameter("@RefreshToken", dto.RefreshToken) 
            }; 
            var loginLogs = await _context.Database
                .SqlQueryRaw<LoginLogDto>(sqlLog, logParams)
                .ToListAsync();

            var loginLog = loginLogs.FirstOrDefault();

            if (loginLog == null)
            {
                // Try to revoke if expired
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC usp_RevokeExpiredRefreshToken @UserId, @RefreshToken",
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@RefreshToken", dto.RefreshToken)
                );

                throw new BusinessException("INVALID_EXPIRED_TOKEN","Invalid or expired refresh token");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = dto.RefreshToken // Reuse the same refresh token until it expires
            };
        }

        public async Task<string> Logout(int userId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                   "EXEC usp_LogoutAllSession @UserId",
                   new SqlParameter("@UserId", userId)
               );
            return "Logout Successful";
        }
    }
}
