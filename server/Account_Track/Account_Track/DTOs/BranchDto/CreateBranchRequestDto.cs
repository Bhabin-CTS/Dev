using System.ComponentModel.DataAnnotations;

namespace Account_Track.Dtos.BranchDto
{
    public class CreateBranchRequestDto
    {
        [Required(ErrorMessage = "branchName is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "branchName must be between 2 and 100 characters")]
        public string BranchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ifscCode is required")]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "ifscCode must be between 6 and 15 characters")]
        [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "ifscCode must be alphanumeric uppercase")]
        public string IFSCCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "city is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "city must be between 2 and 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "state is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "state must be between 2 and 100 characters")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "country is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "country must be between 2 and 100 characters")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "pinCode is required")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "pinCode must be between 4 and 10 characters")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "pinCode must not contain spaces or special characters")]
        public string Pincode { get; set; } = string.Empty;
    }
}