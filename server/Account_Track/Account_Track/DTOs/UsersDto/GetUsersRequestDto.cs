using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.UsersDto
{
    public class GetUsersRequestDto
    {
        public int? BranchId { get; set; }

        public UserRole? Role { get; set; }

        public UserStatus? Status { get; set; }

        public bool? IsLocked { get; set; }

        public string? NameOrEmailSearch { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public DateTime? UpdatedFrom { get; set; }
        public DateTime? UpdatedTo { get; set; }

        public string? SortBy { get; set; }

        public string? SortOrder { get; set; }

        public int Limit { get; set; } = 20;

        public int Offset { get; set; } = 0;
    }
}
