using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.TransactionDto
{
    public class CreateTnxSpResult
    {
        public int Success { get; set; }
        public string? ErrorCode { get; set; }
        public string Message { get; set; }
        public int? TransactionId { get; set; }
        public TransactionStatus? Status { get; set; }
        public TransactionType? Type{ get; set; }
        public bool? IsHighValue { get; set; }
        public bool? ApprovalRequired { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
