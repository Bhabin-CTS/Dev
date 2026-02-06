// Dtos/UserDto/UserResponse.cs
namespace Account_Track.Dtos.UserDto
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;   // Officer | Manager | Admin
        public int BranchId { get; set; }
        public string Status { get; set; } = "Active";     // Active | Inactive | Locked
        public string? CreatedAt { get; set; }             // ISO-8601 UTC
        public string? UpdatedAt { get; set; }             // ISO-8601 UTC
    }
}