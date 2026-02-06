using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.ApprovalDto
{
    public class ApprovalDecisionDto
    {
        [Required(ErrorMessage = "ApprovalId is required.")]
        public int ApprovalId { get; set; }

        [Required(ErrorMessage = "TransactionId is required.")]
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "Decision is required.")]
        public int Decision { get; set; }

        public string? Comments { get; set; }

        [Required(ErrorMessage = "AccountId is required.")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Transaction Type is required.")]
        public int Type { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "TransactionDate is required.")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "ReviewerId is required.")]
        public int ReviewerId { get; set; }

        [Required(ErrorMessage = "ReviewerName is required.")]
        public string ReviewerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ReviewerRole is required.")]
        public int ReviewerRole { get; set; }
        public int TotalCount { get; set; }
    }
}