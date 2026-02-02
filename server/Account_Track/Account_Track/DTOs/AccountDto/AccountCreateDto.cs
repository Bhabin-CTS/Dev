using System.ComponentModel.DataAnnotations;
using Account_Track.Utils.Enum;

namespace Account_Track.DTOs
{
    public class AccountCreateDto
    {
        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
        public required string CustomerName { get; set; }

        [Required(ErrorMessage = "CustomerID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerID must be a positive number.")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "AccountType is required.")]
        [EnumDataType(typeof(AccountType), ErrorMessage = "AccountType must be Savings or Current.")]
        public AccountType AccountType { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Opening balance cannot be negative.")]
        public decimal OpeningBalance { get; set; } = 0m;

        [Required(ErrorMessage = "BranchId is required.")]
        public int BranchId { get; set; }              


    }
}

