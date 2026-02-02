using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Account_Track.Utils.Enum;

namespace Account_Track.Model
{

    [Index(nameof(AccountNumber), IsUnique = true, Name = "IX_Account_Number")]
    [Index(nameof(BranchId), nameof(Status), nameof(AccountType), Name = "IX_Account_Branch_Status_Type")]
    [Index(nameof(CreatedByUserId), nameof(CreatedAt), Name = "IX_Account_User_Created_Date")]
    [Index(nameof(CreatedByUserId), Name = "IX_Account_User_Created")]
    [Table("t_Account")]
    public class t_Account
    {
        [Key]
        public int AccountId { get; set; }

        [Required, MaxLength(100)]
        public required string CustomerName { get; set; }

        [Required]
        public int AccountNumber{ get; set; }

        [Required]
        public AccountType AccountType { get; set; } 

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [Required]
        public AccountStatus Status { get; set; } = AccountStatus.Active;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>(); // prevent null reference error 

        [Required]
        public int BranchId { get; set; }
        [ForeignKey(nameof(BranchId))] //compile time safety 
        public required t_Branch Branch { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }
        [ForeignKey(nameof(CreatedByUserId))] 
        public t_User? CreatedByUser { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } 

        public DateTime? UpdatedAt { get; set; } = null;

        public ICollection<t_Transaction>? Transactions { get; set; }
    }
}
