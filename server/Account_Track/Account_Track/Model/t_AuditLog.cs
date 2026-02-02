using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Account_Track.Model
{

    [Index(nameof(UserId), nameof(CreatedAt), Name = "IX_Audit_User_Date")]
    [Index(nameof(EntityType), nameof(CreatedAt), Name = "IX_Audit_Entity_Date")]
    [Table("t_AuditLog")]
    public class t_AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public t_User? User { get; set; }

        [Required]
        public int LoginId { get; set; }
        [ForeignKey(nameof(LoginId))]
        public t_LoginLog? LoginLog { get; set; }

        [MaxLength(100),Required]
        public required string EntityType {  get; set; }
        
        [Required]
        public required int EntityId {  get; set; }

        [MaxLength(100),Required]
        public required string Action {  get; set; }
        
        [Required, Column(TypeName ="nvarchar(max)")]
        public required string beforeState {  get; set; }

        [Required, Column(TypeName = "nvarchar(max)")]
        public required string afterState { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
