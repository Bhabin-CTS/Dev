using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.Dtos.UserDto
{
    public class UpdateUserRequestDto
    {
        public string? Name { get; set; }

        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid Role")]
        public UserRole? Role { get; set; }

        public int? BranchId { get; set; }
    }
}