using System.ComponentModel.DataAnnotations;

namespace Account_Track.Controllers
{
    public class FirstPasswordResetRequestDto
    {
        [Required,EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
