using Account_Track.DTOs.NotificationDto;

namespace Account_Track.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationListResponseDto>> GetNotificationsAsync(int userId); 
        Task UpdateNotificationsAsync(UpdateNotificationsRequestDto dto, int userId);
    }
}
