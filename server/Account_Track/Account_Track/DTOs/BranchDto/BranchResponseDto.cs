namespace Account_Track.Dtos.BranchDto
{
    public class BranchResponseDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } // ISO-8601
    }
}