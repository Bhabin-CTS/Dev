namespace Account_Track.DTOs.ReportDto
{
    public class TransactionTrendResponseDto
    {
        public DateTime Date { get; set; }

        public int TotalTransactionCount { get; set; }

        public decimal TotalTransactionAmount { get; set; }
    }
}
