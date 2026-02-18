namespace Account_Track.DTOs.AccountDto
{
    public class AccountListItemDto
    {
        public int AccountID { get; set; }
        public int AccountType { get; set; }       // int to match view mapping
        public decimal Balance { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int CustomerID { get; set; }        // int to match view mapping
        public string CustomerName { get; set; } = string.Empty;
        public string RowVersionBase64 { get; set; } = string.Empty;
        public int Status { get; set; }            // int to match view mapping
    }
}