CREATE OR ALTER PROCEDURE usp_Report_Overall_TransactionTrend
(
    @StartDate DATETIME,
    @EndDate   DATETIME,
    @BranchId  INT = NULL   -- Optional
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
      AND Status = 1   -- Completed only (important)
      AND (@BranchId IS NULL OR BranchId = @BranchId)

    GROUP BY CAST(CreatedAt AS DATE)
    ORDER BY [Date]
END

DECLARE @Start DATETIME = DATEADD(MONTH, -3, GETDATE());
DECLARE @End   DATETIME = GETDATE();

EXEC usp_Report_Overall_TransactionTrend 
    @StartDate = @Start,
    @EndDate   = @End;