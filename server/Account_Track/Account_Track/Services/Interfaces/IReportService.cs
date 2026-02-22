using Account_Track.DTOs.ReportDto;

namespace Account_Track.Services.Interfaces
{
    public interface IReportService
    {
        Task<OverallSummaryResponseDto> GetOverallSummaryAsync(OverallSummaryRequestDto dto);
        Task<List<TransactionTrendResponseDto>> GetOverallTransactionTrendAsync(TransactionTrendRequestDto dto);
        Task<List<TxnTypeBreakdownResponseDto>>GetOverallTxnTypeBreakdownAsync(TxnTypeBreakdownRequestDto dto);
        Task<List<AccountGrowthResponseDto>>GetOverallAccountGrowthAsync(AccountGrowthRequestDto dto);
        Task<List<HighValueStatusResponseDto>>GetOverallHighValueStatusAsync(HighValueStatusRequestDto dto);
        Task<List<TopBranchesResponseDto>>GetOverallTopBranchesAsync(TopBranchesRequestDto dto);
    }
}
