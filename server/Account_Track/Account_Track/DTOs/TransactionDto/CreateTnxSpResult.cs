using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.TransactionDto
{
    public class CreateTnxSpResult
    {
        public int Success { get; set; }
        public string? ErrorCode { get; set; }
        public string Message { get; set; }
        public int? TransactionId { get; set; }

        [EnumDataType(typeof(TransactionStatus), ErrorMessage = "Invalid TransactionStatus")]
        public TransactionStatus? Status { get; set; }

        [EnumDataType(typeof(TransactionType), ErrorMessage = "Invalid TransactionType")]
        public TransactionType? Type{ get; set; }
        public bool? IsHighValue { get; set; }
        public bool? ApprovalRequired { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
