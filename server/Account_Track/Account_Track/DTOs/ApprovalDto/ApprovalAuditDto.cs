using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.ApprovalDto
{
    public class ApprovalAuditDto
    {
        [Required(ErrorMessage = "ApprovalID is required.")]
        public int ApprovalID { get; set; }

        [Required(ErrorMessage = "TransactionID is required.")]
        public int TransactionID { get; set; }

        [Required(ErrorMessage = "ReviewerName is required.")]
        public string ReviewerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ReviewerRole is required.")]
        public int ReviewerRole { get; set; }

        [Required(ErrorMessage = "Decision is required.")]
        public int Decision { get; set; }

        [Required(ErrorMessage = "ApprovalDate is required.")]
        public DateTime ApprovalDate { get; set; }

        public string? Comments { get; set; }
    }
}