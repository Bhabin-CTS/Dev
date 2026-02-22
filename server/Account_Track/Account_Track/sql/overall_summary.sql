--This is for overall summary

CREATE OR ALTER PROCEDURE usp_Report_Overall_Summary
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

    ------------------------------------------------------------
    -- PERIOD LOGIC
    ------------------------------------------------------------
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

    ------------------------------------------------------------
    -- TRANSACTION AGGREGATION (ONLY COMPLETED)
    ------------------------------------------------------------
    ;WITH TxnAgg AS
    (
        SELECT
            -- Total Transaction (Completed Only)
            SUM(CASE WHEN CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurTxnCount,
            SUM(CASE WHEN CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevTxnCount,

            SUM(CASE WHEN CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurTxnAmount,
            SUM(CASE WHEN CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevTxnAmount,

            -- Transfer
            SUM(CASE WHEN Type = 1 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurTransferCount,
            SUM(CASE WHEN Type = 1 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevTransferCount,

            SUM(CASE WHEN Type = 1 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurTransferAmount,
            SUM(CASE WHEN Type = 1 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevTransferAmount,

            -- Deposit
            SUM(CASE WHEN Type = 2 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurDepositCount,
            SUM(CASE WHEN Type = 2 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevDepositCount,

            SUM(CASE WHEN Type = 2 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurDepositAmount,
            SUM(CASE WHEN Type = 2 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevDepositAmount,

            -- Withdraw
            SUM(CASE WHEN Type = 3 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurWithdrawCount,
            SUM(CASE WHEN Type = 3 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevWithdrawCount,

            SUM(CASE WHEN Type = 3 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurWithdrawAmount,
            SUM(CASE WHEN Type = 3 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevWithdrawAmount,

            -- High Value
            SUM(CASE WHEN IsHighValue = 1 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN 1 ELSE 0 END) AS CurHighValueCount,
            SUM(CASE WHEN IsHighValue = 1 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN 1 ELSE 0 END) AS PrevHighValueCount,

            SUM(CASE WHEN IsHighValue = 1 AND CreatedAt >= @StartCurrent AND CreatedAt < @EndCurrent THEN Amount ELSE 0 END) AS CurHighValueAmount,
            SUM(CASE WHEN IsHighValue = 1 AND CreatedAt >= @StartPrevious AND CreatedAt < @EndPrevious THEN Amount ELSE 0 END) AS PrevHighValueAmount

        FROM t_Transaction
        WHERE Status = 1  -- ?? IMPORTANT: Only Completed
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

EXEC usp_Report_Overall_Summary 'YEAR';