// Dtos/UserDto/UserResponse.cs
using Account_Track.Utils.Enum;

namespace Account_Track.Dtos.UserDto
{
    public class UserResponseDto
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public UserRole Role { get; set; }

        public int BranchId { get; set; }

        public UserStatus Status { get; set; }

        public bool IsLocked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }           // ISO-8601 UTC
    }
}