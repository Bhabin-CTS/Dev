using Account_Track.DTOs;
using Account_Track.DTOs.AuthDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger  )
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            try
            {
                _logger.LogDebug(dto.Email);

                var result = await _authService.Login(dto);

                return Ok(new ApiResponseDto<LoginResponseDto>
                {
                    Success = true,
                    Data = result,
                    Message = "Login successful",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException be)
            {
                return BadRequest(new ErrorResponseDto
                {
                    ErrorCode = be.ErrorCode,
                    Message = be.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (SqlException)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    ErrorCode = "DATABASE_ERROR",
                    Message = "Database operation failed",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");

                return StatusCode(500, new ErrorResponseDto
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Unexpected error occurred",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto)
        {
            try
            {
                var result = await _authService.RefreshToken(dto);

                return Ok(new ApiResponseDto<LoginResponseDto>
                {
                    Success = true,
                    Data = result,
                    Message = "Token refreshed successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException be)
            {
                return BadRequest(new ErrorResponseDto
                {
                    ErrorCode = be.ErrorCode,
                    Message = be.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");

                return StatusCode(500, new ErrorResponseDto
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Unexpected error occurred",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpPost("logout")]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                int userId = int.Parse(User.FindFirst("UserId").Value);

                var result = await _authService.Logout(userId);

                return Ok(new ApiResponseDto<string>
                {
                    Success = true,
                    Data = result,
                    Message = "Logout successful",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");

                return StatusCode(500, new ErrorResponseDto
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Unexpected error occurred",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}
