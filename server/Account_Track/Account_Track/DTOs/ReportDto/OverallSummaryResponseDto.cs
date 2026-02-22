namespace Account_Track.DTOs.ReportDto
{
    public class OverallSummaryResponseDto
    {
        // Total Transactions
        public int CurTxnCount { get; set; }
        public int PrevTxnCount { get; set; }
        public decimal CurTxnAmount { get; set; }
        public decimal PrevTxnAmount { get; set; }

        // Transfer
        public int CurTransferCount { get; set; }
        public int PrevTransferCount { get; set; }
        public decimal CurTransferAmount { get; set; }
        public decimal PrevTransferAmount { get; set; }

        // Deposit
        public int CurDepositCount { get; set; }
        public int PrevDepositCount { get; set; }
        public decimal CurDepositAmount { get; set; }
        public decimal PrevDepositAmount { get; set; }

        // Withdraw
        public int CurWithdrawCount { get; set; }
        public int PrevWithdrawCount { get; set; }
        public decimal CurWithdrawAmount { get; set; }
        public decimal PrevWithdrawAmount { get; set; }

        // High Value
        public int CurHighValueCount { get; set; }
        public int PrevHighValueCount { get; set; }
        public decimal CurHighValueAmount { get; set; }
        public decimal PrevHighValueAmount { get; set; }

        // Accounts
        public int CurNewAccounts { get; set; }
        public int PrevNewAccounts { get; set; }
        public int ActiveAccounts { get; set; }
    }
}
