using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Account_Track.Model
{
    [Index(nameof(UserId), Name = "IX_Login_UserId")]
    [Index(nameof(UserId), nameof(LoginAt), Name = "IX_Login_User_Date")]
    [Index(nameof(UserId), nameof(RefreshToken), Name = "IX_Login_UserId_RefreshToken")]
    [Index(nameof(RefreshToken), Name = "IX_Login_RefreshToken")]
    [Table("t_LoginLog")]
    public class t_LoginLog
    {
        [Key]
        public int LoginId { get; set; } 

        [Required]
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public required t_User User { get; set; }

        [Required]
        public required DateTime LoginAt { get; set; } = DateTime.UtcNow;

        [MaxLength(255), Required]
        public required string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiry { get; set; }

        public bool IsRevoked { get; set; } = false;

        public ICollection <t_AuditLog>? AuditLogs {  get; set; }
    }
}
