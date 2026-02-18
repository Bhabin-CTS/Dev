using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.Dtos.UserDto
{
    public class UserResponseDto
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid UserRole")]
        public UserRole Role { get; set; }

        public int BranchId { get; set; }
        [EnumDataType(typeof(UserStatus), ErrorMessage = "Invalid UserStatus")]
        public UserStatus Status { get; set; }

        public bool IsLocked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }           // ISO-8601 UTC
    }
}