namespace Account_Track.Dtos.BranchDto
{
    public class BranchResponse
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string? UpdatedAt { get; set; } // ISO-8601
    }
}