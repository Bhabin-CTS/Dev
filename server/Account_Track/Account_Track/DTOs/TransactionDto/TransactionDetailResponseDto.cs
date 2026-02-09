using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.TransactionDto
{
    public class TransactionDetailResponseDto
    {
        public int TransactionId { get; set; }
        public int CreatedBy { get; set; }
        [EnumDataType(typeof(TransactionType), ErrorMessage = "Invalid TransactionType")]
        public TransactionType Type { get; set; } 
        public decimal Amount { get; set; }
        [EnumDataType(typeof(TransactionStatus), ErrorMessage = "Invalid TransactionStatus")]
        public TransactionStatus Status { get; set; }
        public bool IsHighValue { get; set; }
        public int? FromAccountId { get; set; }
        public int? ToAccountId { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
