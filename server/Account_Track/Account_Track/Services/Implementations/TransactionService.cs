using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.DTOs.TransactionDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Account_Track.Utils.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Account_Track.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext db)
        {
            _context = db;
        }

        public async Task<CreateTransactionResponseDto> CreateTransactionAsync(CreateTransactionRequestDto dto, int userId,int loginId)
        {
            // ===== VALIDATION =====
            if (dto.Type == TransactionType.Transfer && dto.FromAccountId == dto.ToAccountId)
                throw new BusinessException("INVALID_REQUEST",
                    "From and To account cannot be same");

            if (dto.Amount <= 0)
                throw new BusinessException("INVALID_AMOUNT",
                    "Transaction amount must be greater than zero");

            if (dto.Type == TransactionType.Transfer && dto.ToAccountId == null)
                throw new BusinessException("INVALID_REQUEST",
                    "ToAccountId is required for transfer transactions");

            if (dto.Type != TransactionType.Transfer && dto.ToAccountId != null)
                throw new BusinessException("INVALID_REQUEST",
                    "ToAccountId should be null for deposit and withdrawal transactions");

            // ===== CALL STORED PROCEDURE =====
            var sql = "EXEC usp_Transaction @Action,@UserId,@LoginId,@FromAccountId,@ToAccountId,@Type,@Amount,@Remarks";
            var parameters = new[] 
            {
                new SqlParameter("@Action", "CREATE"),
                new SqlParameter("@UserId", userId), 
                new SqlParameter("@LoginId", loginId),
                new SqlParameter("@FromAccountId", dto.FromAccountId), 
                new SqlParameter("@ToAccountId", dto.ToAccountId ?? (object)DBNull.Value), 
                new SqlParameter("@Type", dto.Type), 
                new SqlParameter("@Amount", dto.Amount), 
                new SqlParameter("@Remarks", dto.Remarks ?? (object)DBNull.Value) 
            };
            var result = (await _context.Database
                .SqlQueryRaw<CreateTnxSpResult>(sql, parameters)
                .ToListAsync())
                .First();

            // ===== ERROR FROM DB =====
            if (result.Success == 0)
                throw new BusinessException(result.ErrorCode, result.Message);

            // ===== SUCCESS RESPONSE =====
            return new CreateTransactionResponseDto
            {
                TransactionId = result.TransactionId,
                Status = result.Status,
                Type = dto.Type,
                Amount = dto.Amount,
                IsHighValue = result.IsHighValue ?? false,
                ApprovalRequired = result.ApprovalRequired ?? false,
                CreatedAt = result.CreatedAt
            };
        }

        public async Task<(List<TransactionListResponseDto>, PaginationDto)> GetTransactionsAsync(GetTransactionsRequestDto request, int userId)
        {
            if (request.CreatedFrom > request.CreatedTo)
                throw new BusinessException("INVALID_DATE_RANGE", "CreatedFrom cannot be greater than CreatedTo");

            if (request.UpdatedFrom > request.UpdatedTo)
                throw new BusinessException("INVALID_DATE_RANGE", "UpdatedFrom cannot be greater than UpdatedTo");

            var sql = @"EXEC usp_Transaction @Action,@AccountId, @Type, @Status, @IsHighValue, @CreatedFrom, @CreatedTo, @UpdatedFrom, @UpdatedTo, @SortBy, @SortOrder, @Limit, @Offset, @UserId";
            var parameters = new[]
            {
                new SqlParameter("@Action", "LIST"),
                new SqlParameter("@AccountId", request.AccountId ?? (object)DBNull.Value), 
                new SqlParameter("@Type", request.Type ?? (object)DBNull.Value), 
                new SqlParameter("@Status", request.Status ?? (object)DBNull.Value), 
                new SqlParameter("@IsHighValue", request.IsHighValue ?? (object)DBNull.Value), 
                new SqlParameter("@CreatedFrom", request.CreatedFrom ?? (object)DBNull.Value), 
                new SqlParameter("@CreatedTo", request.CreatedTo ?? (object)DBNull.Value), 
                new SqlParameter("@UpdatedFrom", request.UpdatedFrom ?? (object)DBNull.Value), 
                new SqlParameter("@UpdatedTo", request.UpdatedTo ?? (object)DBNull.Value), 
                new SqlParameter("@SortBy", request.SortBy ?? "CreatedAt"), 
                new SqlParameter("@SortOrder", request.SortOrder ?? "DESC"), 
                new SqlParameter("@Limit", request.Limit), 
                new SqlParameter("@Offset", request.Offset), 
                new SqlParameter("@UserId", userId)
            };

            var spresult = await _context.Database
                .SqlQueryRaw<TransactionListSpResultDto>(sql, parameters)   
                .ToListAsync(); 

            int total = spresult.FirstOrDefault()?.TotalCount ?? 0;
            var data = spresult.Select(x => new TransactionListResponseDto
            {
                TransactionId = x.TransactionId,
                Type = x.Type,
                Amount = x.Amount,
                Status = x.Status,
                IsHighValue = x.IsHighValue,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt ?? null
            }).ToList();

            var pagination = new PaginationDto
            {
                Total = total,
                Limit = request.Limit,
                Offset = request.Offset
            };

            return (data, pagination);
        }

        public async Task<TransactionDetailResponseDto> GetTransactionByIdAsync(int transactionId, int userId)
        {
            var sql = "EXEC usp_Transaction @Action,@TransactionId, @UserId"; 
            var parameters = new[] 
            {
                new SqlParameter("@Action", "GET_BY_ID"),
                new SqlParameter("@TransactionId", transactionId),
                new SqlParameter("@UserId", userId) 
            };
            var list = await _context.Database
                .SqlQueryRaw<TransactionDetailResponseDto>(sql, parameters)
                .ToListAsync();

            var result = list.FirstOrDefault();

            if (result == null)
                throw new KeyNotFoundException("TRANSACTION_NOT_FOUND_OR_ACCESS_DENIED");

            return result;

        }
    }

}