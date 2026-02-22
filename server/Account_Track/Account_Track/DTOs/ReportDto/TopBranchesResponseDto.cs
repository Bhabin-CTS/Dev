namespace Account_Track.DTOs.ReportDto
{
    public class TopBranchesResponseDto
    {
        public string BranchName { get; set; } = string.Empty;

        public int TotalTxn { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
