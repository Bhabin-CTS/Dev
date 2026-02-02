namespace Account_Track.DTOs.ApprovalDto
{
    public class ApprovalDecisionDto
    {
        public int ApprovalId { get; set; }        
        public int TransactionId { get; set; }
        public int Decision { get; set; }          
        public string? Comments { get; set; }
        public int AccountId { get; set; }
        public int Type { get; set; } = default!;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = default!;
        public int ReviewerRole { get; set; } = default!;
       }
}
