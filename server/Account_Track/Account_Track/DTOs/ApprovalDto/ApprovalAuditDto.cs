namespace Account_Track.DTOs.ApprovalDto
{
    public class ApprovalAuditDto
    {

        public int ApprovalID { get; set; }
        public int TransactionID { get; set; }
        public string ReviewerName { get; set; } = default!;
        public int ReviewerRole { get; set; } = default!;
        public int Decision {  get; set; }
        public DateTime ApprovalDate { get; set; }
        public string? Comments { get; set; }
    }
}
