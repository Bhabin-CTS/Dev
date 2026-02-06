using Account_Track.DTOs;
using Account_Track.DTOs.ApprovalDto;
public interface IApprovalService
{
    Task UpdateDecisionAsync(int approvalId, int reviewerId, int decision, string? comments);
    Task<List<PendingApprovalDto>> GetPendingApprovalsAsync(int reviewerId);
    Task<List<ApprovalAuditDto>> GetApprovalAuditAsync(int transactionId);
    Task<(List<ApprovalDecisionDto> Data, PaginationDto Pagination)> GetApprovalsAsync(GetApprovalsRequestDto request, int userId);
    Task<ApprovalDecisionDto?> GetApprovalDetailAsync(int approvalId);
}
