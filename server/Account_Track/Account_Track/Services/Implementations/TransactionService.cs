using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.DTOs.TransactionDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Account_Track.Utils.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Account_Track.Services.Implementations
{
    public class TransactionService: ITransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext db)
        {
            _context = db;
        }

        public async Task<CreateTransactionResponseDto> CreateTransactionAsync(CreateTransactionRequestDto dto, int userId)
        {
            var traceId = Guid.NewGuid().ToString();

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
            var result = (await _context.CreateTnxSpResult
            .FromSqlRaw(
                "EXEC usp_CreateTransaction @p0,@p1,@p2,@p3,@p4,@p5",
                userId,
                dto.FromAccountId,
                dto.ToAccountId,
                dto.Type,
                dto.Amount,
                dto.Remarks
            )
            .AsNoTracking()
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

        public async Task<(List<TransactionListResponseDto>, PaginationDto)>GetTransactionsAsync(GetTransactionsRequestDto request,int userId)
        {
            var result = await _context
                .Set<TransactionListResponseDto>()
                .FromSqlRaw(
                    @"EXEC usp_GetTransactions
                    @AccountId,
                    @Type,
                    @Status,
                    @IsHighValue,
                    @FromDate,
                    @ToDate,
                    @Limit,
                    @Offset,
                    @userId",
                    new SqlParameter("@AccountId", request.AccountId ?? (object)DBNull.Value),
                    new SqlParameter("@Type", request.Type ?? (object)DBNull.Value),
                    new SqlParameter("@Status", request.Status ?? (object)DBNull.Value),
                    new SqlParameter("@IsHighValue", request.IsHighValue ?? (object)DBNull.Value),
                    new SqlParameter("@FromDate", request.FromDate ?? (object)DBNull.Value),
                    new SqlParameter("@ToDate", request.ToDate ?? (object)DBNull.Value),
                    new SqlParameter("@Limit", request.Limit),
                    new SqlParameter("@Offset", request.Offset),
                    new SqlParameter("@userId",userId)
                )
                .AsNoTracking()
                .ToListAsync();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            var pagination = new PaginationDto
            {
                Total = total,
                Limit = request.Limit,
                Offset = request.Offset
            };

            return (result, pagination);
        }

        public async Task<TransactionDetailResponseDto> GetTransactionByIdAsync(int transactionId,int userId)
        {
            var list = await _context
            .Set<TransactionDetailResponseDto>()
            .FromSqlRaw(
                "EXEC usp_GetTransactionById @TransactionId, @UserId",
                new SqlParameter("@TransactionId", transactionId),
                new SqlParameter("@UserId", userId)
            )
            .AsNoTracking()
            .ToListAsync();

            var result = list.FirstOrDefault();

            if (result == null)
                throw new KeyNotFoundException("TRANSACTION_NOT_FOUND_OR_ACCESS_DENIED");

            return result;

        }
    }

}
