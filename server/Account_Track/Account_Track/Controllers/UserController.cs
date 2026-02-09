using Account_Track.Dtos.UserDto;
using Account_Track.DTOs;
using Account_Track.DTOs.UsersDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        // CREATE USER
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(CreateUserRequestDto dto)
        {
            try
            {
                int CurrentUserId = int.Parse(User.FindFirst("UserId").Value);
                var data = await _service.CreateUserAsync(dto, CurrentUserId);

                return StatusCode(201, new ApiResponseDto<UserResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "User created successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        // UPDATE USER
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequestDto dto)
        {
            try
            {
                int CurrentUserId = int.Parse(User.FindFirst("UserId").Value);
                var data = await _service.UpdateUserAsync(id, dto, CurrentUserId);

                return Ok(new ApiResponseDto<UserResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "User updated successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        // UPDATE USER STATUS
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus(int id, ChangeUserStatusRequestDto dto)
        {
            try
            {
                int CurrentUserId = int.Parse(User.FindFirst("UserId").Value);
                var data = await _service.UpdateUserStatusAsync(id, dto, CurrentUserId);

                return Ok(new ApiResponseDto<UserResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "User status updated successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        // GET USERS LIST
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequestDto dto)
        {
            try
            {
                int CurrentUserId = int.Parse(User.FindFirst("UserId").Value);
                var (data, pagination) = await _service.GetUsersAsync(dto, CurrentUserId);

                return Ok(new ApiResponseWithPagination<List<UserResponseDto>>
                {
                    Success = true,
                    Data = data,
                    Pagination = pagination,
                    Message = "Users retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        // GET CURRENT USER PROFILE
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                int CurrentUserId = int.Parse(User.FindFirst("UserId").Value);
                var data = await _service.GetUserByIdAsync(CurrentUserId);

                return Ok(new ApiResponseDto<UserResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "Profile retrieved",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException ex)
            {
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        // CHANGE PASSWORD
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto)
        {
            try
            {
                int CurrentUserId = int.Parse(User.FindFirst("UserId").Value);
                await _service.ChangePasswordAsync(dto, CurrentUserId);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Password changed successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Password change failed",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        // CHANGE PASSWORD
        [HttpPost("firstPasswordReset")]
        public async Task<IActionResult> FristReset(FirstPasswordResetRequestDto dto)
        {
            try
            {
                
                await _service.FirstResetAsync(dto);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Password changed successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Password change failed",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}
