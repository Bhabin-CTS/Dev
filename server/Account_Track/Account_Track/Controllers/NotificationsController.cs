using Account_Track.DTOs.NotificationDto;
using Account_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService; 
        public NotificationsController(INotificationService notificationService) 
        { 
            _notificationService = notificationService; 
        }
        
        [HttpGet("getAllNotification")]
        [Authorize]
        public async Task<IActionResult> GetNotifications() 
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var result = await _notificationService.GetNotificationsAsync(userId); 
            return Ok(result); 
        }
        
        [HttpPut("markAsRead")]
        [Authorize]
        public async Task<IActionResult> UpdateNotifications([FromBody] UpdateNotificationsRequestDto dto) 
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            await _notificationService.UpdateNotificationsAsync(dto, userId); 
            return NoContent(); 
        }
    }
}
