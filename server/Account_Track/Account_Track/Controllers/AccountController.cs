using Account_Track.DTOs;
using Account_Track.DTOs.AccountDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils; // BusinessException
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _service;

        public AccountController(IAccountService service)
        {
            _service = service;
        }

        /// <summary>
        /// Create new account (auto-generates AccountNumber, optional initial deposit via txn SP).
        /// </summary>
        [HttpPost("create")]
        [Authorize(Roles = "Officer")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequestDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("UserId")!.Value);
                int logId = int.Parse(User.FindFirst("LoginId")!.Value);

                var data = await _service.CreateAccountAsync(dto, userId, logId);

                return StatusCode(201, new ApiResponseDto<CreateAccountResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "Account created successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException be)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = be.ErrorCode,
                    Message = be.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (SqlException se)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
                    Message = se.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = e.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get paginated accounts (branch-restricted unless Admin). Supports filters/sorting.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> GetAccounts([FromQuery] GetAccountsRequestDto request)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("UserId")!.Value);

                var (items, pagination) = await _service.GetAccountsAsync(request, userId);

                return Ok(new ApiResponseWithPagination<object>
                {
                    Success = true,
                    Data = items,
                    Pagination = pagination,
                    Message = "Accounts retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException be)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = be.ErrorCode,
                    Message = be.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (SqlException se)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
                    Message = se.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = e.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get account detail by Id (branch-restricted unless Admin).
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> GetAccountDetail([FromRoute] int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("UserId")!.Value);

                var data = await _service.GetAccountByIdAsync(id, userId);

                return Ok(new ApiResponseDto<AccountDetailResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "Account details retrieved",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "NOT_FOUND",
                    Message = knf.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException be)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = be.ErrorCode,
                    Message = be.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (SqlException se)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
                    Message = se.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = e.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Update account (optimistic concurrency via RowVersion Base64).
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> UpdateAccount([FromRoute] int id, [FromBody] UpdateAccountRequestDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("UserId")!.Value);
                int logId = int.Parse(User.FindFirst("LoginId")!.Value);


                var data = await _service.UpdateAccountAsync(id, dto, userId, logId);

                return Ok(new ApiResponseDto<AccountDetailResponseDto>
                {
                    Success = true,
                    Data = data,
                    Message = "Account updated successfully",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (BusinessException be)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = be.ErrorCode,
                    Message = be.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (KeyNotFoundException knf)
            {
                // In case SP decides record vanished between reads
                return NotFound(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "NOT_FOUND",
                    Message = knf.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (SqlException se)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
                    Message = se.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = e.Message,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}