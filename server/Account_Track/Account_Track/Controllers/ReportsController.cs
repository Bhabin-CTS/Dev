using Account_Track.DTOs;
using Account_Track.DTOs.ReportDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("overall-summary")]
        public async Task<IActionResult> GetOverallSummary([FromBody] OverallSummaryRequestDto dto)
        {
            try
            {
                var result = await _reportService.GetOverallSummaryAsync(dto);

                return Ok(new ApiResponseDto<OverallSummaryResponseDto>
                {
                    Success = true,
                    Data = result,
                    Message = "Overall summary retrieved successfully",
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
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
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

        [HttpPost("overall-transaction-trend")]
        public async Task<IActionResult> GetOverallTransactionTrend([FromBody] TransactionTrendRequestDto dto)
        {
            try
            {
                var result = await _reportService
                    .GetOverallTransactionTrendAsync(dto);

                return Ok(new ApiResponseDto<List<TransactionTrendResponseDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Transaction trend retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
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

        [HttpPost("overall-txn-type-breakdown")]
        public async Task<IActionResult> GetOverallTxnTypeBreakdown([FromBody] TxnTypeBreakdownRequestDto dto)
        {
            try
            {
                var result = await _reportService
                    .GetOverallTxnTypeBreakdownAsync(dto);

                return Ok(new ApiResponseDto<List<TxnTypeBreakdownResponseDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Transaction type breakdown retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
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

        [HttpPost("overall-account-growth")]
        public async Task<IActionResult> GetOverallAccountGrowth([FromBody] AccountGrowthRequestDto dto)
        {
            try
            {
                var result = await _reportService
                    .GetOverallAccountGrowthAsync(dto);

                return Ok(new ApiResponseDto<List<AccountGrowthResponseDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Account growth retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
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

        [HttpPost("overall-highvalue-status")]
        public async Task<IActionResult> GetOverallHighValueStatus([FromBody] HighValueStatusRequestDto dto)
        {
            try
            {
                var result = await _reportService
                    .GetOverallHighValueStatusAsync(dto);

                return Ok(new ApiResponseDto<List<HighValueStatusResponseDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "High value status report retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
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

        [HttpPost("overall-top-branches")]
        public async Task<IActionResult> GetOverallTopBranches([FromBody] TopBranchesRequestDto dto)
        {
            try
            {
                var result = await _reportService
                    .GetOverallTopBranchesAsync(dto);

                return Ok(new ApiResponseDto<List<TopBranchesResponseDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Top branches report retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
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

    }
}
