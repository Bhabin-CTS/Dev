using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.AuthDto
{
    public class FindUserDto
    {
        public int UserId { get; set; }
        public UserRole Role { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required bool IsLocked { get; set; } = false;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime? UpdatedAt { get; set; }
    }
}
