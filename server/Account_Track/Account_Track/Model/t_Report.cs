using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Account_Track.Model
{
    public class Period
    {
        public DateOnly? Start { get; set; }
        public DateOnly? End { get; set; }
    }
    public class Metrics
    {
        public int? TotalTransactions { get; set; }
        public int? HighValueCount { get; set; }
        public double? AccountGrowthRate { get; set; }
    }
    public class Scope
    {
        public string? Branch { get; set; }
        public string? AccountType { get; set; }
        public Period? Period { get; set; } = new();
    }

    [Index(nameof(BranchId), nameof(GeneratedDate), Name = "IX_Report_Branch_Date")]
    [Index(nameof(GeneratedDate), Name = "IX_Report_Date")]
    [Index(nameof(BranchId), Name = "IX_Report_Branch")]
    [Table("t_Report")]
    public class t_Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public required int BranchId { get; set; }
        [ForeignKey(nameof(BranchId))]
        public t_Branch? Branch { get; set; }

        [Required]
        public Metrics Metrics { get; set; } = new();

        [Required]
        public Scope Scope  { get; set; } = new();

        [Required]
        public DateTime GeneratedDate { get; set; }= DateTime.UtcNow;
    }
}
