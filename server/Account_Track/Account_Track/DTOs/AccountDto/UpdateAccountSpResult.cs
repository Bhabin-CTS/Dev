namespace Account_Track.DTOs.AccountDto
{
    public class UpdateAccountSpResult
    {
        public int Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }

        public int? AccountId { get; set; }
        public int? AccountNumber { get; set; }
        public string? CustomerName { get; set; }
        public int? AccountType { get; set; }
        public int? Status { get; set; }
        public decimal? Balance { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? CreatedByUserId { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? RowVersionBase64 { get; set; }
    }
}
