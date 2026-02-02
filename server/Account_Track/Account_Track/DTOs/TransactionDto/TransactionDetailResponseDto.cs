using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.TransactionDto
{
    public class TransactionDetailResponseDto
    {
        public int TransactionId { get; set; }
        public int Createdby { get; set; }
        public TransactionType Type { get; set; } 
        public decimal Amount { get; set; }
        public TransactionStatus Status { get; set; }
        public bool IsHighValue { get; set; }
        public int? FromAccountId { get; set; }
        public int? ToAccountId { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
