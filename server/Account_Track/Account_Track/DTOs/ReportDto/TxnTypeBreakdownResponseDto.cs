namespace Account_Track.DTOs.ReportDto
{
    public class TxnTypeBreakdownResponseDto
    {
        public string Period { get; set; } = string.Empty;

        public int DepositCount { get; set; }
        public decimal DepositAmount { get; set; }

        public int WithdrawalCount { get; set; }
        public decimal WithdrawalAmount { get; set; }

        public int TransferCount { get; set; }
        public decimal TransferAmount { get; set; }
    }
}
