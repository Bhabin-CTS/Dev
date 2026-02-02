using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Account_Track.Utils.Enum;

namespace Account_Track.Model
{

    [Index(nameof(UserId), nameof(Status), nameof(NotificationId), Name = "IX_Notif_User_Status_NotificationId")]
    [Index(nameof(UserId), Name = "IX_Notif")]
    [Table("t_Notification")]
    public class t_Notification
    {
        [Key] 
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public required t_User User { get; set; }

        [Required, MaxLength(1000)]
        public required string Message { get; set; }

        [Required]
        public Status Status { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = null;
    }
}
