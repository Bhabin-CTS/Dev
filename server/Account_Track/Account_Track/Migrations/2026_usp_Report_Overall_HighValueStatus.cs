using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_Report_Overall_HighValueStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_Report_Overall_HighValueStatus]
            (
                @StartDate DATETIME,
                @EndDate DATETIME,
                @BranchId INT = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    a.Decision,
                    COUNT(*) AS TotalCount,
                    SUM(t.Amount) AS TotalAmount
                FROM t_Approval a
                INNER JOIN t_Transaction t 
                    ON a.TransactionId = t.TransactionID
                WHERE 
                    t.IsHighValue = 1
                    AND a.CreatedAt >= @StartDate
                    AND a.CreatedAt <  @EndDate
                    AND (
                            @BranchId IS NULL 
                            OR t.BranchId = @BranchId
                        )
                GROUP BY a.Decision
                ORDER BY a.Decision
            END;
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[usp_Report_Overall_HighValueStatus];");
        }
    }
}
