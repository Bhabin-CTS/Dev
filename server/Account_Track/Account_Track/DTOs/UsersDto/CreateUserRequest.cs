using System.ComponentModel.DataAnnotations;

namespace Account_Track.Dtos.UserDto
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "email is required")]
        [EmailAddress(ErrorMessage = "email must be a valid email address")]
        [StringLength(320, ErrorMessage = "email must not exceed 320 characters")]
        public string Email { get; set; } = string.Empty;

        // Removed Password — default is set to hash(email) in the service.

        [Required(ErrorMessage = "role is required")]
        [RegularExpression("Officer|Manager|Admin", ErrorMessage = "role must be one of: Officer, Manager, Admin")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "branchId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "branchId must be a positive integer")]
        public int BranchId { get; set; }
    }
}