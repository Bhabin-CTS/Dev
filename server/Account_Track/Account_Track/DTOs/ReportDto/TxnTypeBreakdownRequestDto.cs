namespace Account_Track.DTOs.ReportDto
{
    public class TxnTypeBreakdownRequestDto
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // WEEK | MONTH | YEAR
        public string PeriodType { get; set; } = "WEEK";

        public int? BranchId { get; set; }
    }
}
