using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Report_usp_OverallTxnTypeBreakdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_OverallTxnTypeBreakdown]
            (
                @StartDate DATETIME,
                @EndDate   DATETIME,
                @PeriodType VARCHAR(10) = 'WEEK',
                @BranchId INT = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                ;WITH BaseData AS
                (
                    SELECT
                        Type,
                        Amount,

                        PeriodKey =
                            CASE 
                                WHEN @PeriodType = 'YEAR'
                                    THEN YEAR(CreatedAt)

                                WHEN @PeriodType = 'MONTH'
                                    THEN YEAR(CreatedAt) * 100 + MONTH(CreatedAt)

                                ELSE
                                    YEAR(CreatedAt) * 100 + DATEPART(ISO_WEEK, CreatedAt)
                            END,

                        PeriodLabel =
                            CASE 
                                WHEN @PeriodType = 'YEAR'
                                    THEN CAST(YEAR(CreatedAt) AS VARCHAR)

                                WHEN @PeriodType = 'MONTH'
                                    THEN DATENAME(MONTH, CreatedAt) 
                                         + ' ' + CAST(YEAR(CreatedAt) AS VARCHAR)

                                ELSE
                                    CONCAT('Week ', DATEPART(ISO_WEEK, CreatedAt),
                                           ' - ', YEAR(CreatedAt))
                            END

                    FROM t_Transaction
                    WHERE CreatedAt >= @StartDate
                      AND CreatedAt < DATEADD(DAY,1,@EndDate)
                      AND Status = 1
                      AND (@BranchId IS NULL OR BranchId = @BranchId)
                )

                SELECT
                    PeriodLabel AS Period,

                    SUM(CASE WHEN Type = 2 THEN 1 ELSE 0 END) AS DepositCount,
                    SUM(CASE WHEN Type = 2 THEN Amount ELSE 0 END) AS DepositAmount,

                    SUM(CASE WHEN Type = 3 THEN 1 ELSE 0 END) AS WithdrawalCount,
                    SUM(CASE WHEN Type = 3 THEN Amount ELSE 0 END) AS WithdrawalAmount,

                    SUM(CASE WHEN Type = 1 THEN 1 ELSE 0 END) AS TransferCount,
                    SUM(CASE WHEN Type = 1 THEN Amount ELSE 0 END) AS TransferAmount

                FROM BaseData

                GROUP BY PeriodKey, PeriodLabel
                ORDER BY PeriodKey
            END;
            ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[usp_OverallTxnTypeBreakdown];");
        }
    }
}
