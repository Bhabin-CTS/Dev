CREATE OR ALTER PROCEDURE usp_Report_Overall_AccountGrowth
(
    @PeriodType VARCHAR(10) = 'MONTH',   -- WEEK | MONTH | YEAR
    @StartDate DATETIME,
    @EndDate DATETIME,
    @BranchId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    ------------------------------------------------------------
    --  Align Start & End To Correct Period Boundary
    ------------------------------------------------------------
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
    ELSE  -- MONTH
    BEGIN
        SET @StartPeriod = DATEFROMPARTS(YEAR(@StartDate),MONTH(@StartDate),1);
        SET @EndPeriod   = DATEFROMPARTS(YEAR(@EndDate),MONTH(@EndDate),1);
    END

    ------------------------------------------------------------
    -- Opening Balance (Accounts Before StartDate)
    ------------------------------------------------------------
    DECLARE @OpeningBalance INT;

    SELECT @OpeningBalance = COUNT(*)
    FROM t_Account
    WHERE CreatedAt < @StartDate
      AND (@BranchId IS NULL OR BranchId = @BranchId);

    ------------------------------------------------------------
    -- Generate All Periods (No Extra Future Period)
    ------------------------------------------------------------
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

    ------------------------------------------------------------
    -- Actual Account Creation Grouping
    ------------------------------------------------------------
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

    ------------------------------------------------------------
    -- Final Output (Only Cumulative Total)
    ------------------------------------------------------------
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
END

DECLARE @Start DATETIME = DATEADD(MONTH,-6,GETDATE());
DECLARE @End   DATETIME = GETDATE();

EXEC usp_Report_Overall_AccountGrowth
    @PeriodType = 'MONTH',
    @StartDate = @Start,
    @EndDate   = @End,
    @BranchId  = NULL

DECLARE @Start DATETIME = DATEADD(YEAR,-5,GETDATE());
DECLARE @End   DATETIME = GETDATE();
EXEC usp_Report_Overall_AccountGrowth
    @PeriodType = 'YEAR',
    @StartDate = @Start,
    @EndDate   = @End,
    @BranchId  = NULL

DECLARE @Start DATETIME = DATEADD(WEEK,-12,GETDATE());
DECLARE @End   DATETIME = GETDATE();
EXEC usp_Report_Overall_AccountGrowth
    @PeriodType = 'WEEK',
    @StartDate = @Start,
    @EndDate   = @End,
    @BranchId  = NULL