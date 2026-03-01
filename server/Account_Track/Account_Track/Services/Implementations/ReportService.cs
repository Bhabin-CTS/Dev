using Account_Track.Data;
using Account_Track.DTOs.ReportDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Account_Track.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OverallSummaryResponseDto> GetOverallSummaryAsync(OverallSummaryRequestDto dto)
        {
            if (dto.PeriodType != "MONTH" && dto.PeriodType != "YEAR")
                throw new BusinessException("INVALID_PERIOD", "PeriodType must be MONTH or YEAR");

            var sql = @"EXEC usp_Report
                        @Action = @Action,
                        @PeriodType = @PeriodType,
                        @BranchId = @BranchId";
            var parameters = new[]
            {
                new SqlParameter("@Action", "SUMMARY"),
                new SqlParameter("@PeriodType", dto.PeriodType),
                new SqlParameter("@BranchId", (object?)dto.BranchId ?? DBNull.Value)
            };

            var list = await _context.Database
                .SqlQueryRaw<OverallSummaryResponseDto>(sql, parameters)
                .ToListAsync();
            var result = list.FirstOrDefault();
            if (result == null)
                throw new BusinessException("NO_DATA", "No summary data found");

            return result;
        }

        public async Task<List<TransactionTrendResponseDto>> GetOverallTransactionTrendAsync(TransactionTrendRequestDto dto)
        {
            if (dto.StartDate > dto.EndDate)
                throw new BusinessException("INVALID_DATE_RANGE",
                    "StartDate cannot be greater than EndDate");

            var sql = @"EXEC usp_Report
                        @Action = @Action,
                        @StartDate = @StartDate,
                        @EndDate = @EndDate,
                        @BranchId = @BranchId";

            var parameters = new[]
            {
                new SqlParameter("@Action", "TXN_TREND"),
                new SqlParameter("@StartDate", dto.StartDate),
                new SqlParameter("@EndDate", dto.EndDate),
                new SqlParameter("@BranchId", (object?)dto.BranchId ?? DBNull.Value)
            };

            var result = await _context.Database
                .SqlQueryRaw<TransactionTrendResponseDto>(sql, parameters)
                .ToListAsync();   // IMPORTANT: No FirstOrDefault()

            return result;
        }

        public async Task<List<TxnTypeBreakdownResponseDto>> GetOverallTxnTypeBreakdownAsync(TxnTypeBreakdownRequestDto dto)
        {
            if (dto.StartDate > dto.EndDate)
                throw new BusinessException("INVALID_DATE_RANGE",
                    "StartDate cannot be greater than EndDate");

            if (dto.PeriodType != "WEEK" &&
                dto.PeriodType != "MONTH" &&
                dto.PeriodType != "YEAR")
                throw new BusinessException("INVALID_PERIOD",
                    "PeriodType must be WEEK, MONTH, or YEAR");

            var sql = @"EXEC usp_Report
                        @Action = @Action,
                        @StartDate = @StartDate,
                        @EndDate = @EndDate,
                        @PeriodType = @PeriodType,
                        @BranchId = @BranchId";

            var parameters = new[]
            {
                new SqlParameter("@Action", "TXN_TYPE_BREAKDOWN"),
                new SqlParameter("@StartDate", dto.StartDate),
                new SqlParameter("@EndDate", dto.EndDate),
                new SqlParameter("@PeriodType", dto.PeriodType),
                new SqlParameter("@BranchId", (object?)dto.BranchId ?? DBNull.Value)
            };

            var result = await _context.Database
                .SqlQueryRaw<TxnTypeBreakdownResponseDto>(sql, parameters)
                .ToListAsync();   // Important: Do NOT use First()

            return result;
        }

        public async Task<List<AccountGrowthResponseDto>> GetOverallAccountGrowthAsync(AccountGrowthRequestDto dto)
        {
            if (dto.StartDate > dto.EndDate)
                throw new BusinessException("INVALID_DATE_RANGE",
                    "StartDate cannot be greater than EndDate");

            if (dto.PeriodType != "WEEK" &&
                dto.PeriodType != "MONTH" &&
                dto.PeriodType != "YEAR")
                throw new BusinessException("INVALID_PERIOD",
                    "PeriodType must be WEEK, MONTH, or YEAR");

            var sql = @"EXEC usp_Report
                        @Action = @Action,
                        @PeriodType = @PeriodType,
                        @StartDate = @StartDate,
                        @EndDate = @EndDate,
                        @BranchId = @BranchId";
            var parameters = new[]
            {
                new SqlParameter("@Action", "ACCOUNT_GROWTH"),
                new SqlParameter("@PeriodType", dto.PeriodType),
                new SqlParameter("@StartDate", dto.StartDate),
                new SqlParameter("@EndDate", dto.EndDate),
                new SqlParameter("@BranchId", (object?)dto.BranchId ?? DBNull.Value)
            };

            var result = await _context.Database
                .SqlQueryRaw<AccountGrowthResponseDto>(sql, parameters)
                .ToListAsync();   // Important: No First()

            return result;
        }

        public async Task<List<HighValueStatusResponseDto>> GetOverallHighValueStatusAsync(HighValueStatusRequestDto dto)
        {
            if (dto.StartDate > dto.EndDate)
                throw new BusinessException("INVALID_DATE_RANGE",
                    "StartDate cannot be greater than EndDate");

            var sql = @"EXEC usp_Report
                        @Action = @Action,
                        @StartDate = @StartDate,
                        @EndDate = @EndDate,
                        @BranchId = @BranchId";
            var parameters = new[]
            {
                new SqlParameter("@Action", "HIGHVALUE_STATUS"),
                new SqlParameter("@StartDate", dto.StartDate),
                new SqlParameter("@EndDate", dto.EndDate),
                new SqlParameter("@BranchId", (object?)dto.BranchId ?? DBNull.Value)
            };

            var result = await _context.Database
                .SqlQueryRaw<HighValueStatusResponseDto>(sql, parameters)
                .ToListAsync();   // Important: No First()

            return result;
        }

        public async Task<List<TopBranchesResponseDto>> GetOverallTopBranchesAsync(TopBranchesRequestDto dto)
        {
            var allowedPeriods = new[] { "WEEK", "MONTH", "YEAR", "OVERALL", "CUSTOM" };
            var allowedRankBy = new[] { "AMOUNT", "COUNT" };

            if (!allowedPeriods.Contains(dto.PeriodType))
                throw new BusinessException("INVALID_PERIOD",
                    "PeriodType must be WEEK, MONTH, YEAR, OVERALL, or CUSTOM");

            if (!allowedRankBy.Contains(dto.RankBy))
                throw new BusinessException("INVALID_RANKBY",
                    "RankBy must be AMOUNT or COUNT");

            if (dto.PeriodType == "CUSTOM")
            {
                if (!dto.StartDate.HasValue || !dto.EndDate.HasValue)
                    throw new BusinessException("INVALID_CUSTOM_RANGE",
                        "StartDate and EndDate are required for CUSTOM period");

                if (dto.StartDate > dto.EndDate)
                    throw new BusinessException("INVALID_DATE_RANGE",
                        "StartDate cannot be greater than EndDate");
            }

            var sql = @"EXEC usp_Report
                        @Action = @Action,
                        @PeriodType = @PeriodType,
                        @RankBy = @RankBy,
                        @StartDate = @StartDate,
                        @EndDate = @EndDate";

            var parameters = new[]
            {
                new SqlParameter("@Action", "TOP_BRANCHES"),
                new SqlParameter("@PeriodType", dto.PeriodType),
                new SqlParameter("@RankBy", dto.RankBy),
                new SqlParameter("@StartDate", (object?)dto.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object?)dto.EndDate ?? DBNull.Value)
            };

            var result = await _context.Database
                .SqlQueryRaw<TopBranchesResponseDto>(sql, parameters)
                .ToListAsync();

            return result;
        }
    }
}
