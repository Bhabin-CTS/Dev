using System.ComponentModel.DataAnnotations;
using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.AccountDto
{
    public class UpdateAccountRequestDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "customerName must be between 2 and 100 characters")]
        public string? CustomerName { get; set; }

        [EnumDataType(typeof(AccountStatus), ErrorMessage = "status must be Active or Closed")]
        public AccountStatus? Status { get; set; }

        [EnumDataType(typeof(AccountType), ErrorMessage = "accountType must be Savings or Current")]
        public AccountType? AccountType { get; set; }


        [MaxLength(500, ErrorMessage = "remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }

        // Rowversion concurrency (Base64 from client)
        [Required(ErrorMessage = "rowVersionBase64 is required")]
        public string RowVersionBase64 { get; set; } = string.Empty;
    }
}