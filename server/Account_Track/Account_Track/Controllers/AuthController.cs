using Account_Track.DTOs.AuthDto;
using Account_Track.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger  )
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            _logger.LogDebug(dto.Email);
            return Ok(await _authService.Login(dto));
        }

        [HttpPost("/refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto)
        {
            return Ok(await _authService.RefreshToken(dto));
        }
    }
}
