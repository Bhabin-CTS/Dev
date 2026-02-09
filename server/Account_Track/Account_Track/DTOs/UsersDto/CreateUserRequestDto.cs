using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.Dtos.UserDto
{
    public class CreateUserRequestDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public int BranchId { get; set; }
    }
}