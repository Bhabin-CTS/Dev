CREATE OR ALTER PROCEDURE usp_Report_Overall_HighValueStatus
(
    @StartDate DATETIME,
    @EndDate DATETIME,
    @BranchId INT = NULL   -- NULL = Overall
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
END

DECLARE @Start DATETIME = DATEADD(MONTH, -3, GETDATE());
DECLARE @End   DATETIME = GETDATE();
EXEC usp_Report_Overall_HighValueStatus
    @StartDate = @Start,
    @EndDate   = @End;

