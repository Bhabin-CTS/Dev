using Account_Track.DTOs.NewFolder;
using Account_Track.Utils.Enum;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthService(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto> Login(LoginRequestDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || user.PasswordHash != dto.Password)
            throw new Exception("Invalid credentials");

        if (user.Status != UserStatus.Active)
            throw new Exception("User not active");

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store access token in DB
        user.AccessToken = accessToken;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

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

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            throw new Exception("Invalid user");

        // YOUR MAIN REQUIREMENT
        if (user.AccessToken != dto.AccessToken)
            throw new Exception("Access token mismatch");

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.AccessToken = newAccessToken;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await _authService.Login(dto);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshToken(dto);
        return Ok(result);
    }
}
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]
                    )
                )
        };
});

app.UseAuthentication();
app.UseAuthorization();

[Authorize(Roles = "Officer")]
[HttpPost]
public IActionResult CreateTransaction()
{
    return Ok();
}

[Authorize(Roles = "Admin,Manager,Officer")]
[HttpGet]
public IActionResult GetTransactions()
{
    return Ok();
}

[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public IActionResult DeleteTransaction(int id)
{
    return Ok();
}
