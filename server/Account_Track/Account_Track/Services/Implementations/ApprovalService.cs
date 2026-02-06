using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.DTOs.ApprovalDto;
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

    public async Task UpdateDecisionAsync(int approvalId, int reviewerId, int decision, string? comments)
    {
        var parameters = new[]
        {
            new SqlParameter("@ApprovalID", approvalId),
            new SqlParameter("@ReviewerID", reviewerId),
            new SqlParameter("@Decision", decision),
            new SqlParameter("@Comments", (object?)comments ?? DBNull.Value),

        };

        try
        {
            _logger.LogInformation("UpdateDecision: Approval={ApprovalId} Reviewer={ReviewerId} Decision={Decision}",
                approvalId, reviewerId, decision);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_UpdateApprovalDecision @ApprovalID, @ReviewerID, @Decision, @Comments",
                parameters);

            _logger.LogInformation("UpdateDecision: OK Approval={ApprovalId} Reviewer={ReviewerId}", approvalId, reviewerId);
        }

        catch (SqlException ex) when (ex.Number == 50001)
        {
            _logger.LogWarning(ex, "Known business/state issue...");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error...");
            throw;
        }
    }

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
            FROM dbo.vw_PendingApprovals
            WHERE ReviewerId = @ReviewerID
              AND Decision   = 1";

        var p = new SqlParameter("@ReviewerID", reviewerId);

        try
        {
            _logger.LogInformation("GetPending: Reviewer={ReviewerId}", reviewerId);
            var list = await _context.Database.SqlQueryRaw<PendingApprovalDto>(sql, p).ToListAsync();
            _logger.LogInformation("GetPending: {Count} items Reviewer={ReviewerId}", list.Count, reviewerId);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPending: error Reviewer={ReviewerId}", reviewerId);
            throw;
        }
    }

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
            FROM dbo.vw_ApprovalAudit
            WHERE TransactionID = @TransactionID
            ORDER BY ApprovalDate DESC";

        var p = new SqlParameter("@TransactionID", transactionId);

        try
        {
            _logger.LogInformation("GetAudit: Tx={TransactionId}", transactionId);
            var list = await _context.Database.SqlQueryRaw<ApprovalAuditDto>(sql, p).ToListAsync();
            _logger.LogInformation("GetAudit: {Count} items Tx={TransactionId}", list.Count, transactionId);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAudit: error Tx={TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<ApprovalDecisionDto?> GetApprovalDetailAsync(int approvalId)
    {
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
            FROM dbo.vw_ApprovalDetail
            WHERE ApprovalId = @ApprovalID";

        var p = new SqlParameter("@ApprovalID", approvalId);

        try
        {
            _logger.LogInformation("GetDetail: Approval={ApprovalId}", approvalId);
            var dto = await _context.Database.SqlQueryRaw<ApprovalDecisionDto>(sql, p).FirstOrDefaultAsync();
            _logger.LogInformation("GetDetail: Found={Found} Approval={ApprovalId}", dto != null, approvalId);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDetail: error Approval={ApprovalId}", approvalId);
            throw;
        }
    }
    public async Task<(List<ApprovalDecisionDto> Data, PaginationDto Pagination)> GetApprovalsAsync(GetApprovalsRequestDto request, int userId)
    {
        // Clamp pagination server-side
        var limit = request.Limit <= 0 ? 20 : Math.Min(request.Limit, 100);
        var offset = Math.Max(request.Offset, 0);

        var sql = @"
            EXEC dbo.usp_GetApprovals
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
            new SqlParameter("@Limit",      limit),
            new SqlParameter("@Offset",     offset),
            new SqlParameter("@SortBy",     (object?)request.SortBy ?? DBNull.Value),
            new SqlParameter("@SortDir",    (object?)request.SortDir ?? DBNull.Value),
            new SqlParameter("@UserId",     userId)
        };

        try
        {
            _logger.LogInformation(
                "GetApprovals: fetching approvals with filters {@Request}, UserId={UserId}, Limit={Limit}, Offset={Offset}",
                request, userId, limit, offset);

            var list = await _context.Database
                                     .SqlQueryRaw<ApprovalDecisionDto>(sql, parameters)
                                     .ToListAsync();

            var total = list.FirstOrDefault()?.TotalCount ?? 0;
            var pagination = new PaginationDto { Total = total, Limit = limit, Offset = offset };

            _logger.LogInformation(
                "GetApprovals: fetched {Count} approvals (total={Total}) for UserId={UserId}",
                list.Count, total, userId);

            return (list, pagination);
        }
        catch (SqlException ex)
        {
            // SQL Server-specific errors (time-outs, deadlocks, constraint violations etc.)
            _logger.LogError(ex,
                "GetApprovals: SQL error while executing usp_GetApprovals for UserId={UserId}. ErrorNumber={Number}, State={State}, Class={Class}",
                userId, ex.Number, ex.State, ex.Class);
            throw; // Re-throw to let upper layers (controller/middleware) convert to appropriate HTTP response
        }
        catch (DbUpdateException ex)
        {
            // Unlikely here since we're reading, but useful if read triggers computed/stored logic
            _logger.LogError(ex,
                "GetApprovals: DB update error encountered during read path for UserId={UserId}", userId);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            // If you add CancellationToken later, this will capture client cancellation/timeouts
            _logger.LogWarning(ex,
                "GetApprovals: operation was canceled for UserId={UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GetApprovals: unexpected error for UserId={UserId}", userId);
            throw;
        }
    }
}
