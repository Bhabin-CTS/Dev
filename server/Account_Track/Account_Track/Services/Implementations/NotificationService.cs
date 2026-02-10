using Account_Track.Data;
using Account_Track.DTOs.NotificationDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Account_Track.Services.Implementations
{
    public class NotificationService: INotificationService
    {
        private readonly ApplicationDbContext _context; 
        public NotificationService(ApplicationDbContext db) 
        { 
            _context = db; 
        }

        public async Task<List<NotificationListResponseDto>> GetNotificationsAsync(int userId) 
        { 
            var sql = "EXEC usp_GetNotifications @UserId"; 
            var parameters = new[] { new SqlParameter("@UserId", userId) }; 
            var spresult = await _context.Database
                .SqlQueryRaw<NotificationListResponseDto>(sql, parameters)
                .ToListAsync();
            return spresult; 
        }
        public async Task UpdateNotificationsAsync(UpdateNotificationsRequestDto dto, int userId) 
        {
            if (dto.NotificationIds == null || !dto.NotificationIds.Any())
                throw new BusinessException("INVALID_REQUEST", "NotificationIds cannot be empty");
            var ids = string.Join(",", dto.NotificationIds);
            var affected =await _context.Database
                            .ExecuteSqlRawAsync(
                            "EXEC usp_UpdateNotifications @UserId, @NotificationIds", 
                            new SqlParameter("@UserId", userId), 
                            new SqlParameter("@NotificationIds", ids));
            if (affected == 0)
                throw new BusinessException("NOT_FOUND", "No notifications updated");
        }
    }
}
