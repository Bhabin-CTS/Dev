using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.AuthDto
{
    public class FindUserDto
    {
        public int UserId { get; set; }
        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid UserRole")]
        public UserRole Role { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required bool IsLocked { get; set; } = false;
        [EnumDataType(typeof(UserStatus), ErrorMessage = "Invalid UserStatus")]
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime? UpdatedAt { get; set; }
    }
}
