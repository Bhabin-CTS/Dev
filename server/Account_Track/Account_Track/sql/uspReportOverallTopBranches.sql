CREATE OR ALTER PROCEDURE usp_Report_Overall_TopBranches
(
    @PeriodType VARCHAR(20) = 'OVERALL',   -- WEEK, MONTH, YEAR, OVERALL, CUSTOM
    @RankBy VARCHAR(20) = 'AMOUNT',        -- AMOUNT or COUNT
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FilterStart DATETIME = NULL;
    DECLARE @FilterEnd DATETIME = NULL;

    ------------------------------------------------------------
    -- PERIOD LOGIC
    ------------------------------------------------------------
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

    ------------------------------------------------------------
    -- BASE DATA (CTE for performance & reuse)
    ------------------------------------------------------------
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

    ------------------------------------------------------------
    -- DYNAMIC RANKING
    ------------------------------------------------------------
    SELECT TOP 5
        BranchName,
        TotalTxn,
        TotalAmount
    FROM BranchData
    ORDER BY
        CASE WHEN @RankBy = 'COUNT' THEN TotalTxn END DESC,
        CASE WHEN @RankBy = 'AMOUNT' THEN TotalAmount END DESC;
END

EXEC usp_Report_Overall_TopBranches 'MONTH', 'AMOUNT'
EXEC usp_Report_Overall_TopBranches 'OVERALL', 'AMOUNT'
EXEC usp_Report_Overall_TopBranches 'MONTH', 'COUNT'
EXEC usp_Report_Overall_TopBranches 'WEEK', 'COUNT'
EXEC usp_Report_Overall_TopBranches 
    @PeriodType = 'CUSTOM',
    @RankBy = 'AMOUNT',
    @StartDate = '2025-01-01',
    @EndDate = '2025-03-31'