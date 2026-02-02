using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Account_Track.Model
{

    [Index(nameof(IFSCCode), IsUnique = true, Name = "IX_Branch_IFSC")]
    [Index(nameof(City), nameof(State), Name = "IX_Branch_City_State")]
    [Table("t_Branch")]
    public class t_Branch
    {
        [Key]
        public int BranchId {  get; set; }

        [MaxLength(100), Required] 
        public required string BranchName { get; set; }

        [MaxLength(50), Required] 
        public required string IFSCCode {  get; set; }
        
        [MaxLength(500), Required]
        public required string City { get; set; }

        [MaxLength(100), Required]
        public required string State { get; set; }

        [MaxLength(100), Required]
        public required string Country { get; set; }

        [MaxLength(20), Required]
        public required string Pincode { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = null; 

        public ICollection<t_User>? Users { get; set; }
        public ICollection<t_Account>? Accounts { get; set; }
        public ICollection<t_Report>? Reports { get; set; }

    }

}
