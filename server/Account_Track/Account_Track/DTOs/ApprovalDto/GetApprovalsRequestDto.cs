namespace Account_Track.DTOs.ApprovalDto
{
    public class GetApprovalsRequestDto
    {

        public int? AccountId { get; set; }
        public int? ReviewerId { get; set; }
        public string? Decision { get; set; }      // e.g., "Approved", "Rejected", "Pending"
        public string? Type { get; set; }          // e.g., "Transfer", "Deposit"
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Sorting + Pagination
        public string? SortBy { get; set; } = "TransactionDate"; // Allowed: TransactionDate, Amount, ApprovalId
        public string? SortDir { get; set; } = "DESC";           // "ASC" | "DESC"
        public int Limit { get; set; } = 20;
        public int Offset { get; set; } = 0;

    }
}
