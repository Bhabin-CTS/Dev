// Dtos/UserDto/UpdateUserRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Account_Track.Dtos.UserDto
{
    public class UpdateUserRequest
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string? Name { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Role must be between 2 and 100 characters")]
        public string? Role { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "branchId must be a positive integer when provided")]
        public int? BranchId { get; set; }
    }
}