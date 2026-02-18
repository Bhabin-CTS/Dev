using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.DTOs.ApprovalDto;
using Account_Track.Utils; // BusinessException
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class ApprovalService : IApprovalService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApprovalService> _logger;

    public ApprovalService(ApplicationDbContext context, ILogger<ApprovalService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Update an approval decision.
    /// Throws BusinessException when SP reports failure or returns no result.
    /// </summary>
    public async Task UpdateDecisionAsync(int approvalId, int reviewerId, int decision, string? comments)
    {
        if (approvalId <= 0)
            throw new BusinessException("INVALID_ID", "ApprovalId must be a positive integer");

        var sql = "EXEC usp_UpdateApprovalDecision @ApprovalID, @ReviewerID, @Decision, @Comments";

        var parameters = new[]
        {
            new SqlParameter("@ApprovalID", approvalId),
            new SqlParameter("@ReviewerID", reviewerId),
            new SqlParameter("@Decision", decision),
            new SqlParameter("@Comments", (object?)comments ?? DBNull.Value),
        };

        // Expect the SP to return a single row indicating success/failure (like transaction SP)
        var result = (await _context.Database
            .SqlQueryRaw<UpdateApprovalSpResult>(sql, parameters)
            .ToListAsync())
            .FirstOrDefault();

        if (result == null)
            throw new BusinessException("NO_SP_RESULT", "No response returned from usp_UpdateApprovalDecision");

        if (result.Success == 0)
            throw new BusinessException(result.ErrorCode ?? "APPROVAL_FAILED",
                                        result.Message ?? "Approval update failed");
    }

    /// <summary>
    /// Returns pending approvals for reviewer. Empty list is OK (no exception).
    /// </summary>
    public async Task<List<PendingApprovalDto>> GetPendingApprovalsAsync(int reviewerId)
    {
        var sql = @"
            SELECT
                ApprovalId,
                TransactionId,
                AccountId,
                [Type],
                Amount,
                ReviewerId,
                Decision
            FROM vw_PendingApprovals
            WHERE ReviewerId = @ReviewerID
              AND Decision   = 1";

        return await _context.Database
            .SqlQueryRaw<PendingApprovalDto>(sql, new SqlParameter("@ReviewerID", reviewerId))
            .ToListAsync();
    }

    /// <summary>
    /// Returns audit entries for a transaction. Empty list is OK (no exception).
    /// </summary>
    public async Task<List<ApprovalAuditDto>> GetApprovalAuditAsync(int transactionId)
    {
        var sql = @"
            SELECT
                ApprovalId,
                TransactionID,
                ReviewerName,
                ReviewerRole,
                Decision,
                ApprovalDate,
                Comments
            FROM vw_ApprovalAudit
            WHERE TransactionID = @TransactionID
            ORDER BY ApprovalDate DESC";

        return await _context.Database
            .SqlQueryRaw<ApprovalAuditDto>(sql, new SqlParameter("@TransactionID", transactionId))
            .ToListAsync();
    }

    /// <summary>
    /// Returns detail by approvalId. Throws KeyNotFoundException if not found (parity with TransactionService).
    /// </summary>
    public async Task<ApprovalDecisionDetailDto?> GetApprovalDetailAsync(int approvalId)
    {
        if (approvalId <= 0)
            throw new BusinessException("INVALID_ID", "ApprovalId must be a positive integer");

        var sql = @"
            SELECT TOP 1
                ApprovalId,
                TransactionId,
                Decision,
                Comments,
                AccountId,
                [Type],
                Amount,
                TransactionDate,
                ReviewerId,
                ReviewerName,
                ReviewerRole
            FROM vw_ApprovalDetail
            WHERE ApprovalId = @ApprovalID";

        var dto = await _context.Database
            .SqlQueryRaw<ApprovalDecisionDetailDto>(sql, new SqlParameter("@ApprovalID", approvalId))
            .FirstOrDefaultAsync();

        if (dto == null)
            throw new KeyNotFoundException("APPROVAL_NOT_FOUND_OR_ACCESS_DENIED");

        return dto;
    }

    /// <summary>
    /// List approvals with pagination. Throws BusinessException for invalid filters (date range).
    /// </summary>
    public async Task<(List<ApprovalDecisionDto> Data, PaginationDto Pagination)> GetApprovalsAsync(GetApprovalsRequestDto request, int userId)
    {
        // Consistent with TransactionService: guard common invalid ranges
        if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate > request.ToDate)
            throw new BusinessException("INVALID_DATE_RANGE", "FromDate cannot be greater than ToDate");

        if (request.MinAmount.HasValue && request.MaxAmount.HasValue && request.MinAmount > request.MaxAmount)
            throw new BusinessException("INVALID_AMOUNT_RANGE", "MinAmount cannot be greater than MaxAmount");

        var sql = @"
            EXEC usp_GetApprovals
                @AccountId,
                @ReviewerId,
                @Decision,
                @Type,
                @MinAmount,
                @MaxAmount,
                @FromDate,
                @ToDate,
                @Limit,
                @Offset,
                @SortBy,
                @SortDir,
                @UserId";

        var parameters = new[]
        {
            new SqlParameter("@AccountId",  (object?)request.AccountId ?? DBNull.Value),
            new SqlParameter("@ReviewerId", (object?)request.ReviewerId ?? DBNull.Value),
            new SqlParameter("@Decision",   (object?)request.Decision ?? DBNull.Value),
            new SqlParameter("@Type",       (object?)request.Type ?? DBNull.Value),
            new SqlParameter("@MinAmount",  (object?)request.MinAmount ?? DBNull.Value),
            new SqlParameter("@MaxAmount",  (object?)request.MaxAmount ?? DBNull.Value),
            new SqlParameter("@FromDate",   (object?)request.FromDate ?? DBNull.Value),
            new SqlParameter("@ToDate",     (object?)request.ToDate ?? DBNull.Value),
            new SqlParameter("@Limit",      request.Limit),
            new SqlParameter("@Offset",     request.Offset),
            new SqlParameter("@SortBy",     request.SortBy ?? (object)DBNull.Value),
            new SqlParameter("@SortDir",    request.SortDir ?? (object)DBNull.Value),
            new SqlParameter("@UserId",     userId)
        };

        var list = await _context.Database
            .SqlQueryRaw<ApprovalDecisionDto>(sql, parameters)
            .ToListAsync();

        int total = list.FirstOrDefault()?.TotalCount ?? 0;

        return (list,
            new PaginationDto
            {
                Total = total,
                Limit = request.Limit,
                Offset = request.Offset
            }
        );
    }
}

