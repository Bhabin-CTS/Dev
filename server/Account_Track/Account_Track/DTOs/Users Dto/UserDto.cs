namespace AccountTrack.DTOs
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public string? Email { get; set; }
        public string? Branch { get; set; }
        public int? BranchId { get; set; }    // Preferred for updates (FK to t_Branch)

        public string? PasswordHash { get; set; }
        public string? Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
