namespace Account_Track.DTOs.ReportDto
{
    public class AccountGrowthRequestDto
    {
        // WEEK | MONTH | YEAR
        public string PeriodType { get; set; } = "MONTH";

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int? BranchId { get; set; }
    }
}
