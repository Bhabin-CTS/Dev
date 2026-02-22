using Account_Track.Utils.Enum;

namespace Account_Track.DTOs.ReportDto
{
    public class HighValueStatusResponseDto
    {
        public TransactionStatus Decision { get; set; } 

        public int TotalCount { get; set; }

        public decimal TotalAmount { get; set; }
    }   
}
