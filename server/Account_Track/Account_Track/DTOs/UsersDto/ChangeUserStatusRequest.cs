using System.ComponentModel.DataAnnotations;

namespace Account_Track.Dtos.UserDto
{
    public class ChangeUserStatusRequest
    {
        [Required(ErrorMessage = "status is required")]
        [RegularExpression("Active|Inactive|Locked", ErrorMessage = "status must be one of: Active, Inactive, Locked")]
        public string Status { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "reason must not exceed 500 characters")]
        public string? Reason { get; set; }
    }
}