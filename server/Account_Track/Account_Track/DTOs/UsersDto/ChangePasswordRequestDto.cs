using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.UsersDto
{
    public class ChangePasswordRequestDto
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
