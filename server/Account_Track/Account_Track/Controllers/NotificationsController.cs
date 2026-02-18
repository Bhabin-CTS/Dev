using Account_Track.DTOs;
using Account_Track.DTOs.NotificationDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")!.Value);
                var result = await _notificationService.GetNotificationsAsync(userId);
                return Ok(new ApiResponseDto<List<NotificationListResponseDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Notifications retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }

        }

        [HttpPut("markAsRead")]
        [Authorize]
        public async Task<IActionResult> UpdateNotifications([FromBody] UpdateNotificationsRequestDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")!.Value);
                await _notificationService.UpdateNotificationsAsync(dto, userId);
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Data = null!,
                    Message = "Notifications marked as read successfully",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "DATABASE_ERROR",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Success = false,
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Server failure",
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                });
            }

        }
    }
}
