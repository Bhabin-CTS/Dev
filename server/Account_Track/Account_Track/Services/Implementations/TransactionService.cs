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

        public async Task<ApiResponseDto<object>> CreateTransactionAsync(CreateTransactionRequestDto dto,int userId)
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
            return new ApiResponseDto<object>
            {
                Success = true,
                Data = new
                {
                    transactionId = result.TransactionId,
                    status = result.Status,
                    amount = dto.Amount,
                    type = dto.Type,
                    isHighValue = result.IsHighValue ?? false,
                    approvalRequired = result.ApprovalRequired ?? false,
                    createdAt = result.CreatedAt
                },
                Message = result.IsHighValue ?? false
                    ? "High-value transaction submitted for approval"
                    : "Transaction completed successfully",
                TraceId = traceId
            };
        }

        public async Task<(List<TransactionListResponseDto>, PaginationDto)>GetTransactionsAsync(GetTransactionsRequestDto request)
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
                    @Offset",
                    new SqlParameter("@AccountId", request.AccountId ?? (object)DBNull.Value),
                    new SqlParameter("@Type", request.Type ?? (object)DBNull.Value),
                    new SqlParameter("@Status", request.Status ?? (object)DBNull.Value),
                    new SqlParameter("@IsHighValue", request.IsHighValue ?? (object)DBNull.Value),
                    new SqlParameter("@FromDate", request.FromDate ?? (object)DBNull.Value),
                    new SqlParameter("@ToDate", request.ToDate ?? (object)DBNull.Value),
                    new SqlParameter("@Limit", request.Limit),
                    new SqlParameter("@Offset", request.Offset)
                )
                .AsNoTracking()
                .ToListAsync();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;
            result.ToList();
            //var data = result.Select(x => new TransactionListResponseDto
            //{
            //    TransactionId = x.TransactionId,
            //    Type = x.Type,
            //    Amount = x.Amount,
            //    Status = x.Status,
            //    IsHighValue = x.IsHighValue,
            //    CreatedAt = x.CreatedAt
            //}).ToList();

            var pagination = new PaginationDto
            {
                Total = total,
                Limit = request.Limit,
                Offset = request.Offset
            };

            return (result, pagination);
        }

        public async Task<TransactionDetailResponseDto> GetTransactionByIdAsync(int transactionId)
        {
            var result = await _context
                .Set<TransactionDetailResponseDto>()
                .FromSqlRaw(
                    "EXEC usp_GetTransactionById @TransactionId",
                    new SqlParameter("@TransactionId", transactionId)
                )
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (result == null)
                throw new KeyNotFoundException("TRANSACTION_NOT_FOUND");

            return result;
        }
    }

}
