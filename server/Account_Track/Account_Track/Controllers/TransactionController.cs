using Account_Track.DTOs;
using Account_Track.DTOs.TransactionDto;
using Account_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _service;

        public TransactionController(ITransactionService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Officer")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequestDto dto)
        {
            int userId = int.Parse(User.FindFirst("UserId").Value);

            var data = await _service.CreateTransactionAsync(dto, userId);

            return StatusCode(201, new ApiResponseDto<CreateTransactionResponseDto>
            {
                Success = true,
                Data = data,
                Message = data.IsHighValue ?? false
                ? "High-value transaction submitted for approval"
                : "Transaction completed successfully",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTransactions([FromQuery] GetTransactionsRequestDto request)
        {
            int userId = int.Parse(User.FindFirst("UserId").Value);
            var (data, pagination) =
                await _service.GetTransactionsAsync(request, userId);

            return Ok(new ApiResponseWithPagination<object>
            {
                Success = true,
                Data = data,
                Pagination = pagination,
                Message = "Transactions retrieved successfully",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTransactionDetail(int id)
        {
            int userId = int.Parse(User.FindFirst("UserId").Value);
            var data = await _service.GetTransactionByIdAsync(id, userId);

            return Ok(new ApiResponseDto<TransactionDetailResponseDto>
            {
                Success = true,
                Data = data,
                Message = "Transaction details retrieved",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }
    }

}
