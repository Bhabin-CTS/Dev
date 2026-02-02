using Microsoft.AspNetCore.Mvc;
using Account_Track.DTOs.ApprovalDto;
namespace Account_Track.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalController : ControllerBase
    {
        private readonly IApprovalService _approvalService;

        // Step 3: Inject Service via Constructor
        public ApprovalController(IApprovalService approvalService)
        {
            _approvalService = approvalService;
        }


        [HttpPut("{approvalId:int}/decision")]
        public async Task<IActionResult> SubmitDecision(int approvalId, [FromBody] ApprovalDecisionDto dto)
        {
            // For now, we trust dto.ReviewerId, but service will VALIDATE mapping
            await _approvalService.UpdateDecisionAsync(
                approvalId,
                dto.ReviewerId,
                dto.Decision,
                dto.Comments
            );

            return Ok("Decision submitted successfully");
        }

        // Example API: Submit approval decision
        [HttpPost("submit")]
        public async Task<IActionResult> Submit(int reviewerId, [FromBody] ApprovalDecisionDto dto)
        {
            await _approvalService.SubmitDecisionAsync(reviewerId, dto);
            return Ok("Decision submitted successfully");
        }

        // Example API: Get pending approvals
        [HttpGet("pending/{reviewerId:int}")]
        public async Task<IActionResult> GetPendingApprovals([FromRoute(Name = "reviewerId")] int reviewerId)
        {
            var pending = await _approvalService.GetPendingApprovalsAsync(reviewerId);
            return Ok(pending);
        }

        // Example API: Get audit log for a transaction
        [HttpGet("audit/{transactionId:int}")]
        public async Task<IActionResult> GetApprovalAudit([FromRoute] int transactionId)
        {
            var audit = await _approvalService.GetApprovalAuditAsync(transactionId);
            return Ok(audit);
        }


        [HttpGet("{approvalId:int}")]
        public async Task<IActionResult> GetApprovalDetail(int approvalId)
        {
            var dto = await _approvalService.GetApprovalDetailAsync(approvalId);
            if (dto == null) return NotFound();
            return Ok(dto);
        }


    }
}