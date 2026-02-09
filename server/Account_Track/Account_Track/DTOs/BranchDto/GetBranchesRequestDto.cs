namespace Account_Track.DTOs.BranchDto
{
    public class GetBranchesRequestDto
    {
        public int? BranchId { get; set; }

        public string? BranchName { get; set; }

        public string? IFSCCode { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Country { get; set; }

        public string? Pincode { get; set; }

        public string? SearchText { get; set; }

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
