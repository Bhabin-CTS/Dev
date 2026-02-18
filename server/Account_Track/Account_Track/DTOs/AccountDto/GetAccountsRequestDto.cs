using System.ComponentModel.DataAnnotations;
using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.AccountDto
{
    public class GetAccountsRequestDto
    {
        public int? AccountNumber { get; set; }

        [EnumDataType(typeof(AccountType))]
        public AccountType? AccountType { get; set; }

        [EnumDataType(typeof(AccountStatus))]
        public AccountStatus? Status { get; set; }

        // Search by customer name (contains)
        [MaxLength(100)]
        public string? Search { get; set; }

        // Date filters on CreatedAt
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Sorting: createdAt | customerName | accountNumber | balance
        [MaxLength(50)]
        public string? SortBy { get; set; }

        // ASC / DESC (default ASC)
        [RegularExpression("^(ASC|DESC)$", ErrorMessage = "sortOrder must be ASC or DESC")]
        public string? SortOrder { get; set; } = "ASC";

        // Pagination (same as Transactions)
        [Range(1, 100)]
        public int Limit { get; set; } = 20;

        [Range(0, int.MaxValue)]
        public int Offset { get; set; } = 0;
    }
}