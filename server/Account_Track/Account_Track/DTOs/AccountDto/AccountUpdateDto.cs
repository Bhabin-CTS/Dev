using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.AccountDto
{
    public class AccountUpdateDto
    {

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
        public string CustomerName { get; set; } = default!;

        [Required(ErrorMessage = "AccountType is required.")]
        [EnumDataType(typeof(AccountType), ErrorMessage = "AccountType must be Savings or Current.")]
        public AccountType AccountType { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(AccountStatus), ErrorMessage = "Status must be Active or Closed.")]
        public AccountStatus Status { get; set; }

        [Required(ErrorMessage = "Balance is required.")]
        [Range(0, 9999999999999.99, ErrorMessage = "Balance cannot be negative.")]
        public decimal Balance { get; set; }
        public int BranchId { get; set; }


        // Optimistic concurrency token passed as Base64 from the client.
        [Required(ErrorMessage = "RowVersion is required.")]
        [MinLength(1, ErrorMessage = "RowVersion is required.")]
        public string RowVersion { get; set; } = string.Empty; // Base64
    }
}
