using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.Dtos.UserDto
{
    public class ChangeUserStatusRequestDto
    {
        [EnumDataType(typeof(UserStatus), ErrorMessage = "Invalid Status")]
        public UserStatus? Status { get; set; }

        public bool? IsLocked { get; set; }

        public string? Reason { get; set; }
    }
}