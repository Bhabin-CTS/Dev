using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.TransactionDto
{
    public class CreateTransactionResponseDto
    {
        public int? TransactionId { get; set; }
        [EnumDataType(typeof(TransactionStatus), ErrorMessage = "Invalid TransactionStatus")]
        public TransactionStatus? Status { get; set; }
        [EnumDataType(typeof(TransactionType), ErrorMessage = "Invalid TransactionType")]
        public TransactionType? Type { get; set; }

        public decimal? Amount { get; set; }

        public bool? IsHighValue { get; set; }

        public bool? ApprovalRequired { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
