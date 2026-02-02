using System.Data;
using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.DTOs.AccountDto;
using Account_Track.Services.Interfaces;
using Microsoft.Data.SqlClient; // <- important
using Microsoft.EntityFrameworkCore;

namespace Account_Track.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountService> _logger; //TO UNDERSTAND THE ERROR OR LOGS ARE  FROM WHICH class

        public AccountService(ApplicationDbContext context, ILogger<AccountService> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<(bool Success, string? Error, AccountListItemDto? Data)> CreateAccountAsync(AccountCreateDto dto)
        {
            try
            {
                if (dto.OpeningBalance < 0)
                    return (false, "Opening balance cannot be negative.", null);

                var branchExists = await _context.Branches.AnyAsync(b => b.BranchId == dto.BranchId);
                if (!branchExists)
                    return (false, "Invalid BranchId.", null);


                var pName = new SqlParameter("@CustomerName", dto.CustomerName.Trim());
                var pCustId = new SqlParameter("@CustomerID", dto.CustomerID);
                var pType = new SqlParameter("@AccountType", (int)dto.AccountType);
                var pBal = new SqlParameter("@OpeningBalance", dto.OpeningBalance);
                var pBranch = new SqlParameter("@BranchId", dto.BranchId);                
                var pOutId = new SqlParameter("@NewAccountID", SqlDbType.Int) { Direction = ParameterDirection.Output };

                // Execute SP to insert and capture new ID
                var sql = "EXEC dbo.usp_CreateAccount @CustomerName, @CustomerID, @AccountType, @OpeningBalance, @BranchId, @NewAccountID OUT";
                await _context.Database.ExecuteSqlRawAsync(sql, pName, pCustId, pType, pBal, pBranch,pOutId);

                var newIdObj = pOutId.Value;
                if (newIdObj == null || newIdObj == DBNull.Value)
                    return (false, "Failed to create account (ID not returned).", null);

                var newId = (int)newIdObj;
                var idParam = new SqlParameter("@AccountID", newId);
                var row = await _context
                    .Set<AccountListItemDto>()
                    .FromSqlRaw("SELECT * FROM dbo.vw_AccountList WHERE AccountID = @AccountID", idParam)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();


                if (row == null)
                    return (false, "Account created but could not be loaded.", null);

                return (true, null, row);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DB error while creating account for CustomerID {CustomerID}", dto.CustomerID);
                return (false, "Failed to create account due to a database error.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating account for CustomerID {CustomerID}", dto.CustomerID);
                return (false, $"Unexpected error: {ex.Message}", null);
            }
        }

        // -------------------------------------------------
        // UPDATE: PUT vi/Account/{id}/edit (via SP + concurrency)
        // -------------------------------------------------
        public async Task<(bool Success, string? Error, AccountListItemDto? Data)> UpdateAccountAsync(int accountId, AccountUpdateDto dto)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(dto.RowVersion))
                    return (false, "RowVersion is required.", null);
                // Convert RowVersion from Base64 to VARBINARY(8)
                byte[] incomingVersion;
                try
                {
                    incomingVersion = Convert.FromBase64String(dto.RowVersion);
                }
                catch (FormatException)
                {
                    return (false, "Invalid RowVersion.", null);
                }

                if (dto.Balance < 0)
                    return (false, "Balance cannot be negative.", null);

                var branchExists = await _context.Branches.AnyAsync(b => b.BranchId == dto.BranchId);
                if (!branchExists)
                    return (false, "Invalid BranchId.", null);


                // Execute SP
                var pId = new SqlParameter("@AccountID", accountId);
                var pName = new SqlParameter("@CustomerName", dto.CustomerName.Trim());
                var pType = new SqlParameter("@AccountType", (int)dto.AccountType);
                var pStatus = new SqlParameter("@Status", (int)dto.Status);
                var pBal = new SqlParameter("@Balance", SqlDbType.Decimal)
                {
                    Precision = 18,
                    Scale = 2,
                    Value = dto.Balance
                };

                var pBranch = new SqlParameter("@BranchId", dto.BranchId);
                var pRv = new SqlParameter("@RowVersion", SqlDbType.VarBinary, 8) { Value = incomingVersion };
                var pOut = new SqlParameter("@ResultCode", SqlDbType.Int) { Direction = ParameterDirection.Output };

                var sql = "EXEC dbo.usp_UpdateAccount @AccountID, @CustomerName, @AccountType, @Status, @Balance,@BranchId, @RowVersion, @ResultCode OUT";
                await _context.Database.ExecuteSqlRawAsync(sql, pId, pName, pType, pStatus, pBal, pBranch, pRv, pOut);

                var code = (pOut.Value is int i) ? i : -1;
                if (code == 1)
                    return (false, "Account not found.", null);
                if (code == 2)
                    return (false, "The account was modified by another user. Please refresh and try again.", null);
                if (code != 0)
                    return (false, "Failed to update account.", null);

                // Success -> load fresh row from the view (with new RowVersion)

                var row = await _context.Database
                            .SqlQueryRaw<AccountListItemDto>("SELECT * FROM dbo.vw_AccountList WHERE AccountID = @AccountID", pId)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();


                if (row == null)
                    return (false, "Account updated but could not be loaded.", null);

                return (true, null, row);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "DB error while updating AccountID {AccountID}", accountId);
                return (false, "Failed to update account due to a database error.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating AccountID {AccountID}", accountId);
                return (false,ex.Message , null);
            }
        }


        public async Task<(bool Success, string? Error, AccountListItemDto? Data)> GetAccountByIdAsync(int accountId)
        {
            try
            {
                var pId = new SqlParameter("@AccountID", accountId);
                var row = await _context.Database
                    .SqlQueryRaw<AccountListItemDto>(  //.sqlQuery and //SqlQueryRaw diff
                        "SELECT * FROM dbo.vw_AccountList WHERE AccountID = @AccountID", pId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (row == null)
                    return (false, $"Account {accountId} not found.", null);

                return (true, null, row);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting AccountID {AccountID}", accountId);
                return (false, "Unexpected error while fetching account.", null);
            }
        }


        // -------------------------------------------------
        // LIST: GET vi/Account (via SP)
        // -------------------------------------------------
        public async Task<(bool Success, string? Error, List<AccountListItemDto>? Data)> GetAllAccountsAsync()
        {
            try
            {
                var rows = await _context
                    .Set<AccountListItemDto>()
                    .FromSqlRaw("SELECT * FROM dbo.vw_AccountList")
                    .AsNoTracking()
                    .ToListAsync();

                return (true, null, rows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving accounts.");
                return (false, "Failed to retrieve accounts.", null);
            }
        }
    }
}
