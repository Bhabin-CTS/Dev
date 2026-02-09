using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.NotificationDto
{
    public class NotificationListResponseDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty; 
        public NotificationType Type { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
