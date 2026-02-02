using Account_Track.Data;
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

    public async Task SubmitDecisionAsync(int reviewerId, ApprovalDecisionDto dto)
    {
        var parameters = new[]
        {
            new SqlParameter("@TransactionID", dto.TransactionId),
            new SqlParameter("@ReviewerID", reviewerId),
            new SqlParameter("@Decision", dto.Decision),
            new SqlParameter("@Comments", (object?)dto.Comments ?? DBNull.Value),
        };

        try
        {
            _logger.LogInformation("SubmitDecision: Tx={TransactionId} Reviewer={ReviewerId} Decision={Decision}",
                dto.TransactionId, reviewerId, dto.Decision);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_CreateApproval @TransactionID, @ReviewerID, @Decision, @Comments",
                parameters);

            _logger.LogInformation("SubmitDecision: OK Tx={TransactionId} Reviewer={ReviewerId}", dto.TransactionId, reviewerId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SubmitDecision: SQL error Tx={TransactionId} Reviewer={ReviewerId}", dto.TransactionId, reviewerId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubmitDecision: Unexpected error Tx={TransactionId} Reviewer={ReviewerId}", dto.TransactionId, reviewerId);
            throw;
        }
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
            _logger.LogWarning(ex, "UpdateDecision: Not allowed/state invalid Approval={ApprovalId} Reviewer={ReviewerId}",
                approvalId, reviewerId);
            throw; // middleware/controller maps to 403/409
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "UpdateDecision: SQL error Approval={ApprovalId} Reviewer={ReviewerId}", approvalId, reviewerId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateDecision: Unexpected error Approval={ApprovalId} Reviewer={ReviewerId}", approvalId, reviewerId);
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
              AND Decision   = 0";

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
}
