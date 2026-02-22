namespace Account_Track.DTOs.ReportDto
{
    public class TopBranchesRequestDto
    {
        // WEEK | MONTH | YEAR | OVERALL | CUSTOM
        public string PeriodType { get; set; } = "OVERALL";

        // AMOUNT | COUNT
        public string RankBy { get; set; } = "AMOUNT";

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
