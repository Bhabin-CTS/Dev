using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.UsersDto
{
    public class UserListSpResultDto
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public UserRole Role { get; set; }

        public int BranchId { get; set; }

        public UserStatus Status { get; set; }

        public bool IsLocked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int TotalCount { get; set; }   // 👈 This maps COUNT(*) OVER()
    }
}
