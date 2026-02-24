using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Report_usp_OverallAccountGrowth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_OverallAccountGrowth]
            (
                @PeriodType VARCHAR(10) = 'MONTH',
                @StartDate DATETIME,
                @EndDate DATETIME,
                @BranchId INT = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @StartPeriod DATETIME,
                        @EndPeriod   DATETIME;

                IF @PeriodType = 'YEAR'
                BEGIN
                    SET @StartPeriod = DATEFROMPARTS(YEAR(@StartDate),1,1);
                    SET @EndPeriod   = DATEFROMPARTS(YEAR(@EndDate),1,1);
                END
                ELSE IF @PeriodType = 'WEEK'
                BEGIN
                    SET @StartPeriod = DATEADD(WEEK, DATEDIFF(WEEK,0,@StartDate),0);
                    SET @EndPeriod   = DATEADD(WEEK, DATEDIFF(WEEK,0,@EndDate),0);
                END
                ELSE
                BEGIN
                    SET @StartPeriod = DATEFROMPARTS(YEAR(@StartDate),MONTH(@StartDate),1);
                    SET @EndPeriod   = DATEFROMPARTS(YEAR(@EndDate),MONTH(@EndDate),1);
                END

                DECLARE @OpeningBalance INT;

                SELECT @OpeningBalance = COUNT(*)
                FROM t_Account
                WHERE CreatedAt < @StartDate
                  AND (@BranchId IS NULL OR BranchId = @BranchId);

                ;WITH Periods AS
                (
                    SELECT @StartPeriod AS PeriodStart

                    UNION ALL

                    SELECT
                        CASE 
                            WHEN @PeriodType = 'YEAR'
                                THEN DATEADD(YEAR,1,PeriodStart)
                            WHEN @PeriodType = 'WEEK'
                                THEN DATEADD(WEEK,1,PeriodStart)
                            ELSE
                                DATEADD(MONTH,1,PeriodStart)
                        END
                    FROM Periods
                    WHERE PeriodStart < @EndPeriod
                ),

                AccountGrouped AS
                (
                    SELECT
                        CASE 
                            WHEN @PeriodType = 'YEAR'
                                THEN DATEFROMPARTS(YEAR(CreatedAt),1,1)
                            WHEN @PeriodType = 'WEEK'
                                THEN DATEADD(WEEK, DATEDIFF(WEEK,0,CreatedAt),0)
                            ELSE
                                DATEFROMPARTS(YEAR(CreatedAt),MONTH(CreatedAt),1)
                        END AS PeriodStart,

                        COUNT(*) AS NewAccounts
                    FROM t_Account
                    WHERE CreatedAt BETWEEN @StartDate AND @EndDate
                      AND (@BranchId IS NULL OR BranchId = @BranchId)
                    GROUP BY
                        CASE 
                            WHEN @PeriodType = 'YEAR'
                                THEN DATEFROMPARTS(YEAR(CreatedAt),1,1)
                            WHEN @PeriodType = 'WEEK'
                                THEN DATEADD(WEEK, DATEDIFF(WEEK,0,CreatedAt),0)
                            ELSE
                                DATEFROMPARTS(YEAR(CreatedAt),MONTH(CreatedAt),1)
                        END
                )

                SELECT
                    CASE 
                        WHEN @PeriodType = 'YEAR'
                            THEN CAST(YEAR(p.PeriodStart) AS VARCHAR)
                        WHEN @PeriodType = 'WEEK'
                            THEN CONCAT(
                                    YEAR(p.PeriodStart),
                                    ' W',
                                    DATEPART(WEEK,p.PeriodStart)
                                 )
                        ELSE
                            FORMAT(p.PeriodStart,'MMM yyyy')
                    END AS PeriodLabel,

                    @OpeningBalance +
                    SUM(ISNULL(a.NewAccounts,0)) OVER (
                        ORDER BY p.PeriodStart
                        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                    ) AS TotalAccounts

                FROM Periods p
                LEFT JOIN AccountGrouped a
                    ON p.PeriodStart = a.PeriodStart
                ORDER BY p.PeriodStart
                OPTION (MAXRECURSION 1000);
            END;
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS [dbo].[usp_OverallAccountGrowth];");
        }
    }
}
