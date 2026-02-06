using Account_Track.DTOs;
using Account_Track.DTOs.ApprovalDto;
using Account_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ApprovalController : ControllerBase
    {
        private readonly IApprovalService _approvalService;

        // Step 3: Inject Service via Constructor
        public ApprovalController(IApprovalService approvalService)
        {
            _approvalService = approvalService;
        }


        [HttpPut("{approvalId:int}/decision")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SubmitDecision(
           int approvalId,
           [FromBody] SubmitDecisionRequestDto dto)
        {
            int reviewerId = int.Parse(User.FindFirst("UserId").Value);

            await _approvalService.UpdateDecisionAsync(
                approvalId,
                //dto.ReviewerId,          // <<< We trust JWT, NOT client
                reviewerId,
                dto.Decision,
                dto.Comments
            );

            var updated = await _approvalService.GetApprovalDetailAsync(approvalId);
            if (updated is null)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Approval not found after update.",
                    Data = null,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }


            return Ok(new ApiResponseDto<ApprovalDecisionDto>
            {
                Success = true,
                Message = "Decision submitted successfully",
                Data = updated,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }

        // Example API: Get pending approvals
        [HttpGet("pending")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            int reviewerId = int.Parse(User.FindFirst("UserId").Value);

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

        // Example API: Get audit log for a transaction
       [HttpGet("audit/{transactionId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApprovalAudit(int transactionId)
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


        [HttpGet]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetApprovalDetail([FromQuery] GetApprovalsRequestDto request)
        {

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Invalid or missing UserId claim",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
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


    }
}