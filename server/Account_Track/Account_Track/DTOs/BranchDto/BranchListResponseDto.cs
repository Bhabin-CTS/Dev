namespace Account_Track.DTOs.BranchDto
{
    public class BranchListResponseDto
    {
        public int BranchId { get; set; }

        public string BranchName { get; set; }

        public string IFSCCode { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string Pincode { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
