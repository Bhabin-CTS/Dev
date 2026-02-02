using Account_Track.DTOs.ApprovalDto;
public interface IApprovalService
{
    Task SubmitDecisionAsync(int reviewerId, ApprovalDecisionDto dto);

    Task UpdateDecisionAsync(int approvalId, int reviewerId, int decision, string? comments);

    Task<List<PendingApprovalDto>> GetPendingApprovalsAsync(int reviewerId);

    Task<List<ApprovalAuditDto>> GetApprovalAuditAsync(int transactionId);
    Task<ApprovalDecisionDto?> GetApprovalDetailAsync(int approvalId);
}
