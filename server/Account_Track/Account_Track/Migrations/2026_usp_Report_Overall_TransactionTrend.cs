using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_Report_Overall_TransactionTrend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_Report_Overall_TransactionTrend]
            (
                @StartDate DATETIME,
                @EndDate   DATETIME,
                @BranchId  INT = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    CAST(CreatedAt AS DATE) AS [Date],

                    COUNT(*) AS TotalTransactionCount,
                    ISNULL(SUM(Amount), 0) AS TotalTransactionAmount

                FROM t_Transaction

                WHERE CreatedAt >= @StartDate
                  AND CreatedAt <  DATEADD(DAY, 1, @EndDate)
                  AND Status = 1
                  AND (@BranchId IS NULL OR BranchId = @BranchId)

                GROUP BY CAST(CreatedAt AS DATE)
                ORDER BY [Date]
            END;
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[usp_Report_Overall_TransactionTrend];");
        }
    }
}
