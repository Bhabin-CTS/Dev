using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.DTOs.AccountDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils; // BusinessException
using Account_Track.Utils.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Account_Track.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _db;

        public AccountService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ------------------------------------------------
        // CREATE
        // ------------------------------------------------
        public async Task<CreateAccountResponseDto> CreateAccountAsync(CreateAccountRequestDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                throw new BusinessException("INVALID_REQUEST", "customerName is required");

            if (dto.InitialDeposit < 0)
                throw new BusinessException("INVALID_AMOUNT", "initialDeposit must be >= 0");

            var sql = "EXEC dbo.usp_Account_Create @PerformedByUserId, @CustomerName, @AccountType, @InitialDeposit, @Remarks";
            var parameters = new[]
            {
                new SqlParameter("@PerformedByUserId", userId),
                new SqlParameter("@CustomerName", dto.CustomerName),
                new SqlParameter("@AccountType", (int)dto.AccountType),
                new SqlParameter("@InitialDeposit", dto.InitialDeposit),
                new SqlParameter("@Remarks", (object?)dto.Remarks ?? DBNull.Value)
            };

            var result = (await _db.Database
                .SqlQueryRaw<CreateAccountSpResult>(sql, parameters)
                .ToListAsync())
                .FirstOrDefault();

            if (result == null)
                throw new BusinessException("DB_ERROR", "Account creation failed: empty result from database");

            if (result.Success == 0)
                throw new BusinessException(result.ErrorCode ?? "DB_ERROR", result.Message ?? "Account creation failed");

            return new CreateAccountResponseDto
            {
                AccountId = result.AccountId ?? 0,
                AccountNumber = result.AccountNumber ?? 0,
                AccountType = (AccountType)(result.AccountType ?? 0),
                Status = (AccountStatus)(result.Status ?? 0),
                Balance = result.Balance ?? 0m,
                CreatedAt = result.CreatedAt ?? DateTime.UtcNow
            };
        }

        // ------------------------------------------------
        // LIST
        // ------------------------------------------------
        public async Task<(List<AccountListResponseDto> Items, PaginationDto Pagination)> GetAccountsAsync(
            GetAccountsRequestDto request, int userId)
        {
            // Parity with Transactions/Approvals: basic range guards
            if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate > request.ToDate)
                throw new BusinessException("INVALID_DATE_RANGE", "FromDate cannot be greater than ToDate");

            if (request.Limit <= 0)
                throw new BusinessException("INVALID_PAGINATION", "Limit must be greater than zero");

            if (request.Offset < 0)
                throw new BusinessException("INVALID_PAGINATION", "Offset cannot be negative");

            var sql = @"EXEC dbo.usp_GetAccounts 
                        @AccountNumber, @AccountType, @Status, @Search,
                        @FromDate, @ToDate, @SortBy, @SortOrder, 
                        @Limit, @Offset, @UserId";

            var parameters = new[]
            {
                new SqlParameter("@AccountNumber", request.AccountNumber ?? (object)DBNull.Value),
                new SqlParameter("@AccountType",  request.AccountType   ?? (object)DBNull.Value),
                new SqlParameter("@Status",       request.Status        ?? (object)DBNull.Value),
                new SqlParameter("@Search",       (object?)request.Search ?? DBNull.Value),
                new SqlParameter("@FromDate",     request.FromDate      ?? (object)DBNull.Value),
                new SqlParameter("@ToDate",       request.ToDate        ?? (object)DBNull.Value),
                new SqlParameter("@SortBy",       (object?)request.SortBy ?? DBNull.Value),
                new SqlParameter("@SortOrder",    (object?)request.SortOrder ?? DBNull.Value),
                new SqlParameter("@Limit",        request.Limit),
                new SqlParameter("@Offset",       request.Offset),
                new SqlParameter("@UserId",       userId)
            };

            var rows = await _db.Database
                .SqlQueryRaw<AccountListResponseDto>(sql, parameters)
                .ToListAsync();

            int total = rows.FirstOrDefault()?.TotalCount ?? 0;

            var pagination = new PaginationDto
            {
                Total = total,
                Limit = request.Limit,
                Offset = request.Offset
            };

            return (rows, pagination);
        }

        // ------------------------------------------------
        // DETAIL
        // ------------------------------------------------
        public async Task<AccountDetailResponseDto> GetAccountByIdAsync(int accountId, int userId)
        {
            if (accountId <= 0)
                throw new BusinessException("INVALID_ID", "AccountId must be a positive integer");

            var sql = "EXEC dbo.usp_GetAccountById @AccountId, @UserId";
            var parameters = new[]
            {
                new SqlParameter("@AccountId", accountId),
                new SqlParameter("@UserId", userId)
            };

            var list = await _db.Database
                .SqlQueryRaw<AccountDetailResponseDto>(sql, parameters)
                .ToListAsync();

            var result = list.FirstOrDefault();

            if (result == null)
                throw new KeyNotFoundException("ACCOUNT_NOT_FOUND_OR_ACCESS_DENIED");

            return result;
        }

        // ------------------------------------------------
        // UPDATE (RowVersion concurrency via Base64)
        // ------------------------------------------------
        public async Task<AccountDetailResponseDto> UpdateAccountAsync(int accountId, UpdateAccountRequestDto dto, int userId)
        {
            if (accountId <= 0)
                throw new BusinessException("INVALID_ID", "AccountId must be a positive integer");

            if (string.IsNullOrWhiteSpace(dto.RowVersionBase64))
                throw new BusinessException("INVALID_REQUEST", "rowVersionBase64 is required");

            var sql = @"
                EXEC dbo.usp_Account_Update 
                    @AccountId=@AccountId,
                    @CustomerName=@CustomerName,
                    @Status=@Status,
                    @Remarks=@Remarks,
                    @RowVersionBase64=@RowVersionBase64,
                    @PerformedByUserId=@PerformedByUserId,
                    @AccountType=@AccountType;";

            var parameters = new[]
            {
                new SqlParameter("@AccountId", accountId),
                new SqlParameter("@CustomerName", (object?)dto.CustomerName ?? DBNull.Value),
                new SqlParameter("@Status", dto.Status ?? (object)DBNull.Value),
                new SqlParameter("@Remarks", (object?)dto.Remarks ?? DBNull.Value),
                new SqlParameter("@RowVersionBase64", dto.RowVersionBase64),
                new SqlParameter("@PerformedByUserId", userId),
                new SqlParameter("@AccountType", dto.AccountType ?? (object)DBNull.Value)
            };

            // Expect the SP to return an envelope row indicating success/failure + projection
            var result = (await _db.Database
                .SqlQueryRaw<UpdateAccountSpResult>(sql, parameters)
                .ToListAsync())
                .FirstOrDefault();

            if (result == null)
                throw new BusinessException("DB_ERROR", "Update failed: empty result from database");

            if (result.Success == 0)
                throw new BusinessException(result.ErrorCode ?? "DB_ERROR", result.Message ?? "Update failed");

            return new AccountDetailResponseDto
            {
                AccountId = result.AccountId ?? accountId,
                AccountNumber = result.AccountNumber ?? 0,
                CustomerName = result.CustomerName ?? string.Empty,
                AccountType = (Account_Track.Utils.Enum.AccountType)(result.AccountType ?? 0),
                Status = (Account_Track.Utils.Enum.AccountStatus)(result.Status ?? 0),
                Balance = result.Balance ?? 0m,
                BranchId = result.BranchId ?? 0,
                BranchName = result.BranchName ?? string.Empty,
                CreatedByUserId = result.CreatedByUserId ?? 0,
                CreatedAt = result.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = result.UpdatedAt,
                RowVersionBase64 = result.RowVersionBase64 ?? string.Empty
            };
        }
    }
}