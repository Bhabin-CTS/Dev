using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Account_Track.Utils.Enum;

namespace Account_Track.Model
{

    [Index(nameof(TransactionId), IsUnique = true, Name = "IX_Approval_Txn")]
    [Index(nameof(Decision), nameof(CreatedAt), Name = "IX_Approval_Decision_Date")]
    [Index(nameof(ReviewerId), nameof(CreatedAt), Name = "IX_Approval_Reviewer_Date")]
    [Index(nameof(Decision), Name = "IX_Approval_Decision")]
    [Index(nameof(ReviewerId), Name = "IX_Approval_Reviewer")]
    [Index(nameof(TransactionId), nameof(ReviewerId), Name = "IX_Approval_TransactionId_ReviewerId")]
    [Table("t_Approval")]
    public class t_Approval
    {
        [Key]
        public int ApprovalId { set; get; }

        [Required]
        public int TransactionId { set; get; }
        [ForeignKey(nameof(TransactionId))]
        public required t_Transaction Transaction { set; get; }

        [Required]
        public int ReviewerId { set; get; }
        [ForeignKey(nameof(ReviewerId))]
        public required t_User Reviewer { set; get; }

        [Required]
        public DecisionType Decision { set; get; } = DecisionType.Pending;

        [MaxLength(500)]
        public string? Comments { set; get; }

        public DateTime? DecidedAt { set; get; } = null;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
