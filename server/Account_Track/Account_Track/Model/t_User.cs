using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Account_Track.Utils.Enum;

namespace Account_Track.Model
{

    [Index(nameof(Email), IsUnique = true, Name = "IX_User_Email")]
    [Index(nameof(Email), nameof(Status), IsUnique = true, Name = "IX_User_Email_Status")]
    [Index(nameof(BranchId), nameof(Role), nameof(Status), Name = "IX_User_Branch_Role_Status")]
    [Index(nameof(IsLocked), nameof(FalseAttempt), Name = "IX_User_Lock_Attempt")]
    [Index(nameof(IsLocked), Name = "IX_User_Locked")]
    [Table("t_User")]
    public class t_User
    {
        [Key]
        public int UserId { get; set; }

        [MaxLength(100), Required]
        public required string Name { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [EmailAddress, MaxLength(100), Required]
        public required string Email { get; set; }

        [Required]
        public int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public required t_Branch Branches { get; set; }

        [MaxLength(255), Required]
        public required string PasswordHash { get; set; }
        
        [Required]
        public required int FalseAttempt { get; set; } = 0;

        [Required]
        public required bool IsLocked { get; set; } = false;
        
        [Required]
        public UserStatus Status { get; set; } = UserStatus.Active;

        [Required]
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = null;

        public ICollection<t_Approval>? Approvals { get; set; }
        public ICollection<t_Notification>? Notifications { get; set; }
        public ICollection<t_AuditLog>? AuditLogs { get; set; }
        public ICollection<t_Account>? Accounts { get; set; }
        public ICollection<t_LoginLog>? LoginLogs { get; set; }
        public ICollection<t_Transaction>? Transactions { get; set; }
    }
}