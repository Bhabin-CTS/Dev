using Account_Track.DTOs;
using Account_Track.DTOs.ApprovalDto;
using Account_Track.Utils; // BusinessException
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ApprovalController : ControllerBase
    {
        private readonly IApprovalService _approvalService;

        public ApprovalController(IApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        [HttpPut("{approvalId:int}/decision")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SubmitDecision(int approvalId, [FromBody] SubmitDecisionRequestDto dto)
        {
            try
            {
                int reviewerId = int.Parse(User.FindFirst("UserId")!.Value);
                int loginId = int.Parse(User.FindFirst("LoginId")!.Value);
                await _approvalService.UpdateDecisionAsync(
                    approvalId,
                    reviewerId, // trust JWT, not client
                    dto.Decision,
                    dto.Comments,
                    loginId
                );

                var updated = await _approvalService.GetApprovalDetailAsync(approvalId);

                return Ok(new ApiResponseDto<ApprovalDecisionDetailDto>
                {
                    Success = true,
                    Message = "Decision submitted successfully",
                    Data = updated!,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
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

        [HttpGet("pending")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                int reviewerId = int.Parse(User.FindFirst("UserId")!.Value);

                var pending = await _approvalService.GetPendingApprovalsAsync(reviewerId);

                return Ok(new ApiResponseDto<List<PendingApprovalDto>>
                {
                    Success = true,
                    Data = pending,
                    Message = "Pending approvals retrieved successfully",
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

        [HttpGet("audit/{transactionId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApprovalAudit(int transactionId)
        {
            try
            {
                var audit = await _approvalService.GetApprovalAuditAsync(transactionId);

                return Ok(new ApiResponseDto<List<ApprovalAuditDto>>
                {
                    Success = true,
                    Data = audit,
                    Message = "Approval audit history retrieved successfully",
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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApprovalDetail([FromQuery] GetApprovalsRequestDto request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")!.Value);
                //var userIdClaim = User.FindFirstValue("UserId");
                //if (!int.TryParse(userIdClaim, out var userId))
                //{
                //    return Unauthorized(new
                //    {
                //        Success = false,
                //        Message = "Invalid or missing UserId claim",
                //        TraceId = HttpContext.TraceIdentifier
                //    });
                //}

                var (data, pagination) = await _approvalService.GetApprovalsAsync(request, userId);

                return Ok(new ApiResponseWithPagination<object>
                {
                    Success = true,
                    Data = data,
                    Pagination = pagination,
                    Message = data.Count == 0 ? "No approvals found" : "Approvals retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
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
    }
}