using Account_Track.DTOs;
using Account_Track.DTOs.AccountDto;
using Account_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            int userId = int.Parse(User.FindFirst("UserId")!.Value);

            var data = await _service.CreateAccountAsync(dto, userId);

            return StatusCode(201, new ApiResponseDto<CreateAccountResponseDto>
            {
                Success = true,
                Data = data,
                Message = "Account created successfully",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Get paginated accounts (branch-restricted unless Admin). Supports filters/sorting.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> GetAccounts([FromQuery] GetAccountsRequestDto request)
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

        /// <summary>
        /// Get account detail by Id (branch-restricted unless Admin).
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> GetAccountDetail([FromRoute] int id)
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

        /// <summary>
        /// Update account (optimistic concurrency via RowVersion Base64).
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> UpdateAccount([FromRoute] int id, [FromBody] UpdateAccountRequestDto dto)
        {
            int userId = int.Parse(User.FindFirst("UserId")!.Value);

            var data = await _service.UpdateAccountAsync(id, dto, userId);

            return Ok(new ApiResponseDto<AccountDetailResponseDto>
            {
                Success = true,
                Data = data,
                Message = "Account updated successfully",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}
