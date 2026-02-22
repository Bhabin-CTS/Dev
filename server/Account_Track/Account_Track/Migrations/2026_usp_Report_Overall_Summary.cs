using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_Report_Overall_Summary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_Report_Overall_Summary]
            (
                @PeriodType VARCHAR(10) = 'MONTH',
                @BranchId INT = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @StartCurrent DATETIME,
                        @EndCurrent DATETIME,
                        @StartPrevious DATETIME,
                        @EndPrevious DATETIME;

                IF @PeriodType = 'YEAR'
                BEGIN
                    SET @StartCurrent  = DATEFROMPARTS(YEAR(GETDATE()),1,1)
                    SET @EndCurrent    = DATEADD(YEAR,1,@StartCurrent)

                    SET @StartPrevious = DATEADD(YEAR,-1,@StartCurrent)
                    SET @EndPrevious   = @StartCurrent
                END
                ELSE
                BEGIN
                    SET @StartCurrent  = DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1)
                    SET @EndCurrent    = DATEADD(MONTH,1,@StartCurrent)

                    SET @StartPrevious = DATEADD(MONTH,-1,@StartCurrent)
                    SET @EndPrevious   = @StartCurrent
                END

                ;WITH TxnAgg AS
                (
                    SELECT
                        SUM(CASE WHEN CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurTxnCount,
                        SUM(CASE WHEN CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevTxnCount,

                        SUM(CASE WHEN CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurTxnAmount,
                        SUM(CASE WHEN CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevTxnAmount,

                        SUM(CASE WHEN Type = 1 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurTransferCount,
                        SUM(CASE WHEN Type = 1 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevTransferCount,

                        SUM(CASE WHEN Type = 1 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurTransferAmount,
                        SUM(CASE WHEN Type = 1 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevTransferAmount,

                        SUM(CASE WHEN Type = 2 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurDepositCount,
                        SUM(CASE WHEN Type = 2 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevDepositCount,

                        SUM(CASE WHEN Type = 2 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurDepositAmount,
                        SUM(CASE WHEN Type = 2 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevDepositAmount,

                        SUM(CASE WHEN Type = 3 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurWithdrawCount,
                        SUM(CASE WHEN Type = 3 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevWithdrawCount,

                        SUM(CASE WHEN Type = 3 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurWithdrawAmount,
                        SUM(CASE WHEN Type = 3 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevWithdrawAmount,

                        SUM(CASE WHEN IsHighValue = 1 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurHighValueCount,
                        SUM(CASE WHEN IsHighValue = 1 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevHighValueCount,

                        SUM(CASE WHEN IsHighValue = 1 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurHighValueAmount,
                        SUM(CASE WHEN IsHighValue = 1 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevHighValueAmount

                    FROM t_Transaction
                    WHERE Status = 1 
                      AND (@BranchId IS NULL OR BranchId = @BranchId)
                ),

                AccountAgg AS
                (
                    SELECT
                        SUM(CASE WHEN CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurNewAccounts,
                        SUM(CASE WHEN CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevNewAccounts,
                        SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS ActiveAccounts
                    FROM t_Account
                    WHERE (@BranchId IS NULL OR BranchId = @BranchId)
                )

                SELECT *
                FROM TxnAgg, AccountAgg
            END;
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[usp_Report_Overall_Summary];");
        }
    }
}
