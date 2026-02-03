using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Account_Track.Utils.Enum;

namespace Account_Track.Model
{

    [Index(nameof(Status), nameof(CreatedAt), Name = "IX_Txn_Status_Date")]
    [Index(nameof(Type), nameof(CreatedAt), Name = "IX_Txn_Type_Date")]
    [Index(nameof(IsHighValue), nameof(Status), Name = "IX_Txn_HighValue_Status")]
    [Index(nameof(IsHighValue), Name = "IX_Txn_HighValue")]
    [Index(nameof(FromAccountId), Name = "IX_Txn_FromAcc")]
    [Index(nameof(ToAccountId), Name = "IX_Txn_ToAcc")]
    [Index(nameof(CreatedByUserId), nameof(CreatedAt), Name = "IX_Txn_User_Date")]
    [Index(nameof(Status), nameof(BranchId), Name = "IX_Txn_Branch")]
    [Table("t_Transaction")]
    public class t_Transaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        public int BranchId { get; set; }
        [ForeignKey(nameof(BranchId))]
        public t_Branch? Branch { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }
        [ForeignKey(nameof(CreatedByUserId))]
        public t_User? CreatedByUser { get; set; }

        [Required]
        public int FromAccountId { get; set; }
        [ForeignKey(nameof(FromAccountId))]
        public t_Account? FromAccount { get; set; }

        public int? ToAccountId { get; set; }
        [ForeignKey(nameof(ToAccountId))]
        public t_Account? ToAccount { get; set; }

        [Required] 
        public TransactionType Type { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")] 
        public decimal Amount { get; set; }

        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

        [Required]
        public bool IsHighValue { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public required int BalanceBefore { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public int? BalanceAfterTxn { get; set; }

        [MaxLength(500)]
        public string? flagReason {  get; set; }
        
        [Required]
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      
        public DateTime? UpdatedAt { get; set; } = null;

        public ICollection<t_Approval>? Approvals { get; set; }
    }
}
