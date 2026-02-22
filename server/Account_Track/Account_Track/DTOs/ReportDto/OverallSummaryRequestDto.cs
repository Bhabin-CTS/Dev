namespace Account_Track.DTOs.ReportDto
{
    public class OverallSummaryRequestDto
    {
        public string PeriodType { get; set; } = "MONTH"; // MONTH or YEAR
        public int? BranchId { get; set; }
    }
}
