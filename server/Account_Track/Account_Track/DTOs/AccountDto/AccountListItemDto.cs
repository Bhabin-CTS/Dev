using Microsoft.EntityFrameworkCore;

namespace Account_Track.DTOs.AccountDto
{
    public class AccountListItemDto
    {
        public int AccountID { get; set; }
        public string CustomerName { get; set; } = default!;
        public int CustomerID { get; set; }
        public int AccountType { get; set; }  // "Savings" | "Current"
        [Precision(18, 2)]
        public decimal Balance { get; set; }
        public int Status { get; set; }      // "Active" | "Closed"
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        //public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public string RowVersionBase64 { get; set; } = string.Empty;



    }
}
