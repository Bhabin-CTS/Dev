using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_Report_Overall_TopBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_Report_Overall_TopBranches]
            (
                @PeriodType VARCHAR(20) = 'OVERALL',
                @RankBy VARCHAR(20) = 'AMOUNT',
                @StartDate DATETIME = NULL,
                @EndDate DATETIME = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @FilterStart DATETIME = NULL;
                DECLARE @FilterEnd DATETIME = NULL;

                IF @PeriodType = 'WEEK'
                BEGIN
                    SET @FilterStart = DATEADD(DAY, -7, GETDATE());
                    SET @FilterEnd   = GETDATE();
                END
                ELSE IF @PeriodType = 'MONTH'
                BEGIN
                    SET @FilterStart = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
                    SET @FilterEnd   = DATEADD(MONTH, 1, @FilterStart);
                END
                ELSE IF @PeriodType = 'YEAR'
                BEGIN
                    SET @FilterStart = DATEFROMPARTS(YEAR(GETDATE()), 1, 1);
                    SET @FilterEnd   = DATEADD(YEAR, 1, @FilterStart);
                END
                ELSE IF @PeriodType = 'CUSTOM'
                BEGIN
                    SET @FilterStart = @StartDate;
                    SET @FilterEnd   = @EndDate;
                END

                ;WITH BranchData AS
                (
                    SELECT
                        b.BranchId,
                        b.BranchName,
                        COUNT(t.TransactionID) AS TotalTxn,
                        SUM(t.Amount) AS TotalAmount
                    FROM t_Transaction t
                    JOIN t_Branch b ON t.BranchId = b.BranchId
                    WHERE
                        (@PeriodType = 'OVERALL')
                        OR
                        (
                            t.CreatedAt >= @FilterStart
                            AND t.CreatedAt < @FilterEnd
                        )
                    GROUP BY b.BranchId, b.BranchName
                )

                SELECT TOP 5
                    BranchName,
                    TotalTxn,
                    TotalAmount
                FROM BranchData
                ORDER BY
                    CASE WHEN @RankBy = 'COUNT' THEN TotalTxn END DESC,
                    CASE WHEN @RankBy = 'AMOUNT' THEN TotalAmount END DESC;
            END;
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[usp_Report_Overall_TopBranches];");
        }
    }
}
