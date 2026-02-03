using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.TransactionDto
{
    public class CreateTransactionResponseDto
    {
        public int? TransactionId { get; set; }

        public TransactionStatus? Status { get; set; }

        public TransactionType? Type { get; set; }

        public decimal? Amount { get; set; }

        public bool? IsHighValue { get; set; }

        public bool? ApprovalRequired { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
