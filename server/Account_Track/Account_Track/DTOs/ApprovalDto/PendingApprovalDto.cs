namespace Account_Track.DTOs.ApprovalDto
{
    public class PendingApprovalDto
    {
        public int ApprovalId { get; set; }
        public int TransactionID { get; set; }
        public int AccountID { get; set; }
        public int Type { get; set; } = default!;
        public decimal Amount { get; set; }
        public int ReviewerID { get; set; }
        public int Decision { get; set; }
    }

}
