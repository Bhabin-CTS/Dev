using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_Report : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
                CREATE OR ALTER PROCEDURE [dbo].[usp_Report]
                (
                    @Action VARCHAR(50),

                    @PeriodType VARCHAR(20) = 'MONTH',
                    @RankBy VARCHAR(20) = 'AMOUNT',

                    @StartDate DATETIME = NULL,
                    @EndDate DATETIME = NULL,

                    @BranchId INT = NULL
                )
                AS
                BEGIN
                    SET NOCOUNT ON;

                    --------------------------------------------------------------------
                    --OVERALL SUMMARY
                    --------------------------------------------------------------------
                    IF @Action = 'SUMMARY'
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

                        RETURN;
                    END

                    --------------------------------------------------------------------
                    -- ACCOUNT GROWTH
                    --------------------------------------------------------------------
                    IF @Action = 'ACCOUNT_GROWTH'
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

                        RETURN;
                    END

                    --------------------------------------------------------------------
                    -- HIGH VALUE STATUS
                    --------------------------------------------------------------------
                    IF @Action = 'HIGHVALUE_STATUS'
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

                        RETURN;
                    END

                    --------------------------------------------------------------------
                    -- TOP BRANCHES
                    --------------------------------------------------------------------
                    IF @Action = 'TOP_BRANCHES'
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

                        RETURN;
                    END

                    --------------------------------------------------------------------
                    -- TRANSACTION TREND
                    --------------------------------------------------------------------
                    IF @Action = 'TXN_TREND'
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

                        RETURN;
                    END

                    --------------------------------------------------------------------
                    -- TRANSACTION TYPE BREAKDOWN
                    --------------------------------------------------------------------
                    IF @Action = 'TXN_TYPE_BREAKDOWN'
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

                        RETURN;
                    END
                END
                ";
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            @"IF OBJECT_ID('[dbo].[usp_Report]', 'P') IS NOT NULL
              DROP PROCEDURE [dbo].[usp_Report];");
        }
    }
}
