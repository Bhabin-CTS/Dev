using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Account_Track.Model
{

    [Index(nameof(UserId), nameof(LoginAt), Name = "IX_Login_User_Date")]
    [Index(nameof(UserId), nameof(LogOutAt), Name = "IX_Logout_User_Date")]
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

        [Required]
        public required DateTime LogOutAt { get; set; } //+token valid time

        public ICollection <t_AuditLog>? AuditLogs {  get; set; }
    }
}
