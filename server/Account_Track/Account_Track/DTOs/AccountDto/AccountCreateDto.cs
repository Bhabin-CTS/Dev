using System;
using System.ComponentModel.DataAnnotations;
using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.AccountDto
{
    public class CreateAccountRequestDto
    {
        [Required(ErrorMessage = "customerName is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "customerName must be between 2 and 100 characters")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "accountType is required")]
        [EnumDataType(typeof(AccountType), ErrorMessage = "accountType must be Savings or Current")]
        public AccountType AccountType { get; set; }

        // Initial deposit optional; 0 allowed.
        [Range(0.00, 999_999_999.99, ErrorMessage = "initialDeposit must be >= 0")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "initialDeposit can have maximum 2 decimal places")]
        public decimal InitialDeposit { get; set; } = 0.00m;

        [MaxLength(500, ErrorMessage = "remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }
    }

    public class CreateAccountResponseDto
    {
        public int AccountId { get; set; }
        public int AccountNumber { get; set; }
        public AccountType AccountType { get; set; }
        public AccountStatus Status { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// (Optional) If you want SP result surface like Transaction.CreateTnxSpResult.
    /// Keep this here to avoid creating a 4th file for AccountDto.
    /// </summary>
}