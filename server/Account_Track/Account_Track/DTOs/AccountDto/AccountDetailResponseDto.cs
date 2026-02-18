using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.AccountDto
{
    public class AccountDetailResponseDto
    {
        public int AccountId { get; set; }
        public int AccountNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public AccountStatus Status { get; set; }
        public decimal Balance { get; set; }

        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Concurrency token (rowversion -> Base64)
        public string RowVersionBase64 { get; set; } = string.Empty;
    }
}
