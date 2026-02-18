using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.AccountDto
{
    public class AccountListResponseDto
    {
        public int AccountId { get; set; }
        public int AccountNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public AccountStatus Status { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }

        // For pagination consistency (COUNT(*) OVER() AS TotalCount)
        public int TotalCount { get; set; }
    }
}
