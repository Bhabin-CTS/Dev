using Account_Track.DTOs;
using Account_Track.DTOs.TransactionDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _service;

        public TransactionController(ITransactionService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequestDto dto)
        {
            try
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

        [HttpGet]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> GetTransactions([FromQuery] GetTransactionsRequestDto request)
        {
            try
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
 
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Officer,Admin,Manager")]
        public async Task<IActionResult> GetTransactionDetail(int id)
        {
            try
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
