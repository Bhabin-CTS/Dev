namespace Account_Track.DTOs.ReportDto
{
    public class TransactionTrendRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? BranchId { get; set; }
    }
}
