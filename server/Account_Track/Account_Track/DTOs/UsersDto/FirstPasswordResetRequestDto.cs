using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.UsersDto
{
    public class FirstPasswordResetRequestDto
    {
        [Required,EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string OldPassword { get; set; }

        [Required]
        public required string NewPassword { get; set; }
    }
}
