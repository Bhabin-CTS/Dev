// File: Account_Track/Controllers/UsersController.cs
using Account_Track.DTOs;
using Account_Track.Dtos.UserDto;
using Account_Track.Services.Interfaces;
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
        public UsersController(IUserService service) => _service = service;

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var claim = User.FindFirst("UserId")?.Value;
            return !string.IsNullOrWhiteSpace(claim) && int.TryParse(claim, out userId);
        }

        // POST v1/users (Admin) - default password = hash(email)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest dto)
        {
            try
            {
                if (!TryGetUserId(out var performedBy))
                {
                    return Unauthorized(new ErrorResponseDto
                    {
                        Success = false,
                        ErrorCode = "UNAUTHORIZED",
                        Message = "Missing or invalid UserId claim",
                        TraceId = HttpContext.TraceIdentifier,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var data = await _service.CreateUserAsync(dto, performedBy);

                return StatusCode(201, new ApiResponseDto<UserResponse>
                {
                    Success = true,
                    Data = data,
                    Message = "User created successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            // SP custom duplicate or unique constraint
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627 || ex.Number == 50010)
            {
                return StatusCode(409, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DUPLICATE_EMAIL",
                    Message = "email already exists",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            // SP custom: branch not present
            catch (SqlException ex) when (ex.Number == 50003)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INVALID_REQUEST",
                    Message = "branchId not present",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INVALID_REQUEST",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // PUT v1/users/{id} (Admin) - update name/role/branchId and reset password to hash(email)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserRequest dto)
        {
            try
            {
                if (!TryGetUserId(out var performedBy))
                {
                    return Unauthorized(new ErrorResponseDto
                    {
                        Success = false,
                        ErrorCode = "UNAUTHORIZED",
                        Message = "Missing or invalid UserId claim",
                        TraceId = HttpContext.TraceIdentifier,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var data = await _service.UpdateUserAsync(id, dto, performedBy);

                return Ok(new ApiResponseDto<UserResponse>
                {
                    Success = true,
                    Data = data,
                    Message = "User updated successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "userId not present",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex) when (ex.Number == 50004) // userId not found (SP)
            {
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "userId not present",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex) when (ex.Number == 50003) // branch not present (SP)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INVALID_REQUEST",
                    Message = "branchId not present",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return StatusCode(409, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DUPLICATE_EMAIL",
                    Message = "email already exists",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INVALID_REQUEST",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // PUT v1/users/{id}/status (Admin) - update status/locked + reason
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus([FromRoute] int id, [FromBody] ChangeUserStatusRequest dto)
        {
            try
            {
                if (!TryGetUserId(out var performedBy))
                {
                    return Unauthorized(new ErrorResponseDto
                    {
                        Success = false,
                        ErrorCode = "UNAUTHORIZED",
                        Message = "Missing or invalid UserId claim",
                        TraceId = HttpContext.TraceIdentifier,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var data = await _service.UpdateUserStatusAsync(id, dto, performedBy);

                return Ok(new ApiResponseDto<UserResponse>
                {
                    Success = true,
                    Data = data,
                    Message = "User status updated successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "userId not present",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex) when (ex.Number == 50004)
            {
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "userId not present",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INVALID_REQUEST",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // GET v1/users (Admin) - list with filters + paging
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int? branchId,
            [FromQuery] string? role,
            [FromQuery] string? status,
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortBy,     // name|email|createdAt
            [FromQuery] string? sortOrder,  // ASC|DESC
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            try
            {
                if (!TryGetUserId(out var performedBy))
                {
                    return Unauthorized(new ErrorResponseDto
                    {
                        Success = false,
                        ErrorCode = "UNAUTHORIZED",
                        Message = "Missing or invalid UserId claim",
                        TraceId = HttpContext.TraceIdentifier,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var (data, pagination) = await _service.GetUsersAsync(
                    branchId, role, status, searchTerm, sortBy, sortOrder, limit, offset, performedBy);

                return Ok(new ApiResponseWithPagination<object>
                {
                    Success = true,
                    Data = data,
                    Pagination = pagination,
                    Message = "Users retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INVALID_REQUEST",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // GET v1/users/me (Officer/Manager/Admin)
        [HttpGet("me")]
        [Authorize(Roles = "Officer,Manager,Admin")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                if (!TryGetUserId(out var userId))
                {
                    return Unauthorized(new ErrorResponseDto
                    {
                        Success = false,
                        ErrorCode = "UNAUTHORIZED",
                        Message = "Missing or invalid UserId claim",
                        TraceId = HttpContext.TraceIdentifier,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var data = await _service.GetUserByIdAsync(userId, userId);

                return Ok(new ApiResponseDto<UserResponse>
                {
                    Success = true,
                    Data = data,
                    Message = "Current user profile retrieved",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "userId not present",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}