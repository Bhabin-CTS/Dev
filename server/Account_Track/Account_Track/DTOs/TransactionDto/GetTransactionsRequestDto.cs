using Account_Track.Utils.Enum;
using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.TransactionDto
{
    public class GetTransactionsRequestDto
    {
        public int? AccountId { get; set; }

        [EnumDataType(typeof(TransactionType))]
        public TransactionType? Type { get; set; }

        [EnumDataType(typeof(TransactionStatus))]
        public TransactionStatus? Status { get; set; }

        public bool? IsHighValue { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? UpdatedFrom { get; set; }
        public DateTime? UpdatedTo { get; set; }
        public string? SortBy { get; set; } = "CreatedAt"; 
        public string? SortOrder { get; set; } = "DESC";

        [Range(1, 100)]
        public int Limit { get; set; } = 20;

        [Range(0, int.MaxValue)]
        public int Offset { get; set; } = 0;
    }
}
