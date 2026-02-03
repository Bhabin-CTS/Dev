using Account_Track.DTOs;
using Account_Track.DTOs.TransactionDto;

namespace Account_Track.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<CreateTransactionResponseDto> CreateTransactionAsync(CreateTransactionRequestDto dto, int userId);
        Task<(List<TransactionListResponseDto>, PaginationDto)> GetTransactionsAsync(GetTransactionsRequestDto request, int userId);
        Task<TransactionDetailResponseDto> GetTransactionByIdAsync(int transactionId, int userId);
    }
}
