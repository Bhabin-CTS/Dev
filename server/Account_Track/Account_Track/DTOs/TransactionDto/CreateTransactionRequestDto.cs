using System.ComponentModel.DataAnnotations;
using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.TransactionDto 
{
    public class CreateTransactionRequestDto
    {
        [Required(ErrorMessage = "From Account Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "From Account Id must be a valid positive number")]
        public int FromAccountId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "To Account Id must be a valid positive number")]
        public int? ToAccountId { get; set; }

        [Required(ErrorMessage = "Transaction Type is required")]
        [EnumDataType(typeof(TransactionType),ErrorMessage = "Transaction Type must be Deposit, Withdrawal, or Transfer")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 999999999.99, ErrorMessage = "Amount must be greater than 0")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$",ErrorMessage = "Amount can have maximum 2 decimal places")]
        public decimal Amount { get; set; }

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string? Remarks { get; set; }
    }
}
