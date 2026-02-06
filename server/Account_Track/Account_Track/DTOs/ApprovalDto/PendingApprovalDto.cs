using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.ApprovalDto
{
    public class PendingApprovalDto
    {
        [Required(ErrorMessage = "ApprovalId is required.")]
        public int ApprovalId { get; set; }

        [Required(ErrorMessage = "TransactionID is required.")]
        public int TransactionID { get; set; }

        [Required(ErrorMessage = "AccountID is required.")]
        public int AccountID { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public int Type { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "ReviewerID is required.")]
        public int ReviewerID { get; set; }

        [Required(ErrorMessage = "Decision is required.")]
        public int Decision { get; set; }
    }
}