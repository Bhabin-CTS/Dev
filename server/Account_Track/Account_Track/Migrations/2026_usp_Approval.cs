using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_Approval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE OR ALTER PROCEDURE [dbo].[usp_Approval]
(
    @Action        VARCHAR(20),

    ------------------------------------------------------------
    -- Common (security / identity)
    ------------------------------------------------------------
    @UserId        INT = NULL,            -- actor for security & auditing
    @LoginId       INT = NULL,            -- OPTIONAL: login/session id for auditing

    ------------------------------------------------------------
    -- UPDATE action parameters (reviewer decision)
    ------------------------------------------------------------
    @ApprovalId    INT = NULL,
    @ReviewerId    INT = NULL,            -- MUST match t_Approval.ReviewerId
    @Decision      INT = NULL,            -- 1=Pending, 2=Approve, 3=Reject
    @Comments      NVARCHAR(500) = NULL,

    ------------------------------------------------------------
    -- LIST action filters
    ------------------------------------------------------------
    @AccountId     INT = NULL,
    @ReviewerIdF   INT = NULL,            -- filter by reviewer
    @DecisionF     INT = NULL,            -- filter by decision
    @Type          INT = NULL,            -- 1=Deposit, 2=Withdrawal, 3=Transfer
    @MinAmount     DECIMAL(18,2) = NULL,
    @MaxAmount     DECIMAL(18,2) = NULL,
    @FromDate      DATE = NULL,
    @ToDate        DATE = NULL,
    @Limit         INT = 20,
    @Offset        INT = 0,
    @SortBy        SYSNAME = NULL,        -- whitelist enforced below
    @SortDir       VARCHAR(4) = NULL      -- 'ASC' | 'DESC'
)
AS
BEGIN
    SET NOCOUNT ON;

    ------------------------------------------------------------
    -- ROLES & STATUS CONSTANTS
    ------------------------------------------------------------
    DECLARE @ROLE_ADMIN   INT = 3;
    DECLARE @ROLE_MANAGER INT = 2;
    DECLARE @ROLE_OFFICER INT = 1;

    DECLARE @DEC_PENDING  INT = 1,
            @DEC_APPROVE  INT = 2,
            @DEC_REJECT   INT = 3;

    DECLARE @TXN_COMPLETED INT = 1,
            @TXN_PENDING   INT = 2,
            @TXN_REJECTED  INT = 3;

    ------------------------------------------------------------
    -- USER CONTEXT (used by LIST + optional security)
    ------------------------------------------------------------
    DECLARE @UserBranchId INT, @UserRole INT;

    IF @UserId IS NOT NULL
    BEGIN
        SELECT 
            @UserBranchId = U.BranchId,
            @UserRole     = U.[Role]
        FROM dbo.t_User AS U
        WHERE U.UserId = @UserId;
    END

    ------------------------------------------------------------
    -- ACTION: UPDATE (Reviewer updates approval decision)
    ------------------------------------------------------------
    IF UPPER(@Action) = 'UPDATE'
    BEGIN
        SET NOCOUNT ON;
        SET XACT_ABORT ON;

        -- ===== Business validations: return envelope row (no THROW) =====
        IF NOT EXISTS (SELECT 1 FROM dbo.t_Approval WHERE ApprovalId = @ApprovalId)
        BEGIN
            SELECT 0 AS Success, 'APPROVAL_NOT_FOUND' AS ErrorCode, N'Approval not found.' AS Message;
            RETURN;
        END

        DECLARE
            @TxId               INT,
            @CurrentReviewerId  INT,
            @CurrentDecision    INT;

        SELECT
            @TxId              = A.TransactionId,
            @CurrentReviewerId = A.ReviewerId,
            @CurrentDecision   =  TRY_CONVERT(INT, A.Decision)
        FROM dbo.t_Approval AS A
        WHERE A.ApprovalId = @ApprovalId;

        IF @CurrentReviewerId <> @ReviewerId
        BEGIN
            SELECT 0 AS Success, 'REVIEWER_MISMATCH' AS ErrorCode, N'Reviewer mismatch for this approval.' AS Message;
            RETURN;
        END

        IF @Decision NOT IN (@DEC_PENDING, @DEC_APPROVE, @DEC_REJECT)
        BEGIN
            SELECT 0 AS Success, 'INVALID_DECISION' AS ErrorCode, N'Allowed: 1=Pending, 2=Approve, 3=Reject.' AS Message;
            RETURN;
        END

        IF @Decision = @CurrentDecision
        BEGIN
            SELECT 0 AS Success, 'NO_CHANGE' AS ErrorCode, N'New decision is the same as current.' AS Message;
            RETURN;
        END

        IF @CurrentDecision <> @DEC_PENDING
        BEGIN
            SELECT 0 AS Success, 'ALREADY_FINALIZED' AS ErrorCode, N'Approval is already finalized and cannot be changed.' AS Message;
            RETURN;
        END

        BEGIN TRY
            BEGIN TRAN;

            ------------------------------------------------------------
            -- AUDIT (APPROVAL): capture BEFORE state
            ------------------------------------------------------------
            DECLARE @ApprovalBeforeState NVARCHAR(MAX), @ApprovalAfterState NVARCHAR(MAX);
            SELECT @ApprovalBeforeState = (
                SELECT *
                FROM dbo.t_Approval
                WHERE ApprovalId = @ApprovalId
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            );

            ------------------------------------------------------------
            -- 1) Update the approval row
            ------------------------------------------------------------
            UPDATE dbo.t_Approval
               SET Decision  = @Decision,
                   Comments  = @Comments,
                   DecidedAt = CASE WHEN @Decision <> @DEC_PENDING THEN SYSUTCDATETIME() ELSE NULL END
             WHERE ApprovalId = @ApprovalId;

            ------------------------------------------------------------
            -- AUDIT (APPROVAL): capture AFTER state and insert
            ------------------------------------------------------------
            SELECT @ApprovalAfterState = (
                SELECT *
                FROM dbo.t_Approval
                WHERE ApprovalId = @ApprovalId
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            );

            INSERT INTO dbo.t_AuditLog
            (
                UserId, LoginId, EntityType, EntityId, Action,
                beforeState, afterState, CreatedAt
            )
            VALUES
            (
                @ReviewerId, @LoginId, 'Approval', @ApprovalId, 'UPDATE',
                @ApprovalBeforeState, @ApprovalAfterState, SYSUTCDATETIME()
            );

            ------------------------------------------------------------
            -- Variables for transaction processing + notification
            ------------------------------------------------------------
            DECLARE
                @TypeL            INT,
                @Amount           DECIMAL(18,2),
                @FromAccountId    INT,
                @ToAccountId      INT,
                @TxnStatus        INT,
                @FromBal          DECIMAL(18,2),
                @ToBal            DECIMAL(18,2),
                @NewFromBal       DECIMAL(18,2),
                @NewToBal         DECIMAL(18,2),
                @InitiatorUserId  INT,
                @DecisionText     NVARCHAR(20),
                @TypeText         NVARCHAR(20);

            SET @DecisionText = CASE @Decision
                                   WHEN @DEC_APPROVE THEN N'approved'
                                   WHEN @DEC_REJECT  THEN N'rejected'
                                   ELSE N'pending'
                                END;

            IF @Decision = @DEC_APPROVE
            BEGIN
                -- Lock transaction row while we read & update (UPDLOCK)
                SELECT
                    @TypeL           = T.[Type],
                    @Amount          = T.Amount,
                    @FromAccountId   = T.FromAccountId,
                    @ToAccountId     = T.ToAccountId,
                    @TxnStatus       = T.[Status],
                    @InitiatorUserId = T.CreatedByUserId
                FROM dbo.t_Transaction AS T WITH (UPDLOCK, ROWLOCK)
                WHERE T.TransactionId = @TxId;

                IF @TypeL IS NULL
                BEGIN
                    ROLLBACK TRAN;
                    SELECT 0 AS Success, 'TX_NOT_FOUND' AS ErrorCode, N'Transaction not found for this approval.' AS Message; 
                    RETURN;
                END

                IF @TxnStatus <> @TXN_PENDING
                BEGIN
                    ROLLBACK TRAN;
                    SELECT 0 AS Success, 'TX_NOT_PENDING' AS ErrorCode, N'Transaction is not in Pending state.' AS Message;
                    RETURN;
                END

                IF @Amount <= 0
                BEGIN
                    ROLLBACK TRAN;
                    SELECT 0 AS Success, 'INVALID_AMOUNT' AS ErrorCode, N'Transaction amount must be positive.' AS Message;
                    RETURN;
                END

                -- Lock source account
                SELECT @FromBal = A.Balance
                FROM dbo.t_Account AS A WITH (UPDLOCK, ROWLOCK)
                WHERE A.AccountId = @FromAccountId;

                IF @FromBal IS NULL
                BEGIN
                    ROLLBACK TRAN;
                    SELECT 0 AS Success, 'FROM_NOT_FOUND' AS ErrorCode, N'From account not found.' AS Message;
                    RETURN;
                END

                -- Transfer needs destination
                IF @TypeL = 3
                BEGIN
                    IF @ToAccountId IS NULL
                    BEGIN
                        ROLLBACK TRAN;
                        SELECT 0 AS Success, 'TO_REQUIRED' AS ErrorCode, N'Transfer requires a valid ToAccountId.' AS Message;
                        RETURN;
                    END

                    IF @ToAccountId = @FromAccountId
                    BEGIN
                        ROLLBACK TRAN;
                        SELECT 0 AS Success, 'SAME_ACCOUNTS' AS ErrorCode, N'Transfer requires distinct From and To accounts.' AS Message;
                        RETURN;
                    END

                    SELECT @ToBal = A.Balance
                    FROM dbo.t_Account AS A WITH (UPDLOCK, ROWLOCK)
                    WHERE A.AccountId = @ToAccountId;

                    IF @ToBal IS NULL
                    BEGIN
                        ROLLBACK TRAN;
                        SELECT 0 AS Success, 'TO_NOT_FOUND' AS ErrorCode, N'To account not found.' AS Message;
                        RETURN;
                    END
                END

                --------------------------------------------------------
                -- AUDIT (TRANSACTION): capture BEFORE state
                --------------------------------------------------------
                DECLARE @TxBeforeState NVARCHAR(MAX), @TxAfterState NVARCHAR(MAX);
                SELECT @TxBeforeState = (
                    SELECT *
                    FROM dbo.t_Transaction
                    WHERE TransactionId = @TxId
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                );

                -- Apply balances + mark txn completed
                IF @TypeL = 1 /* Deposit */
                BEGIN
                    SET @NewFromBal = @FromBal + @Amount;

                    UPDATE dbo.t_Account
                       SET Balance = @NewFromBal
                     WHERE AccountId = @FromAccountId;

                    UPDATE dbo.t_Transaction
                       SET [Status]        = @TXN_COMPLETED,
                           BalanceAfterTxn = @NewFromBal,
                           UpdatedAt       = SYSUTCDATETIME()
                     WHERE TransactionId = @TxId;

                    SET @TypeText = N'Deposit';
                END
                ELSE IF @TypeL = 2 /* Withdrawal */
                BEGIN
                    IF @FromBal < @Amount
                    BEGIN
                        ROLLBACK TRAN;
                        SELECT 0 AS Success, 'INSUFFICIENT_FUNDS' AS ErrorCode, N'Insufficient funds for withdrawal.' AS Message;
                        RETURN;
                    END

                    SET @NewFromBal = @FromBal - @Amount;

                    UPDATE dbo.t_Account
                       SET Balance = @NewFromBal
                     WHERE AccountId = @FromAccountId;

                    UPDATE dbo.t_Transaction
                       SET [Status]        = @TXN_COMPLETED,
                           BalanceAfterTxn = @NewFromBal,
                           UpdatedAt       = SYSUTCDATETIME()
                     WHERE TransactionId = @TxId;

                    SET @TypeText = N'Withdrawal';
                END
                ELSE IF @TypeL = 3 /* Transfer */
                BEGIN
                    IF @FromBal < @Amount
                    BEGIN
                        ROLLBACK TRAN;
                        SELECT 0 AS Success, 'INSUFFICIENT_FUNDS' AS ErrorCode, N'Insufficient funds for transfer.' AS Message;
                        RETURN;
                    END

                    SET @NewFromBal = @FromBal - @Amount;
                    SET @NewToBal   = @ToBal   + @Amount;

                    UPDATE dbo.t_Account
                       SET Balance = @NewFromBal
                     WHERE AccountId = @FromAccountId;

                    UPDATE dbo.t_Account
                       SET Balance = @NewToBal
                     WHERE AccountId = @ToAccountId;

                    UPDATE dbo.t_Transaction
                       SET [Status]        = @TXN_COMPLETED,
                           BalanceAfterTxn = @NewFromBal,
                           UpdatedAt       = SYSUTCDATETIME()
                     WHERE TransactionId = @TxId;

                    SET @TypeText = N'Transfer';
                END
                ELSE
                BEGIN
                    ROLLBACK TRAN;
                    SELECT 0 AS Success, 'UNKNOWN_TX_TYPE' AS ErrorCode, N'Unknown transaction type.' AS Message;
                    RETURN;
                END

                --------------------------------------------------------
                -- AUDIT (TRANSACTION): capture AFTER state and insert
                --------------------------------------------------------
                SELECT @TxAfterState = (
                    SELECT *
                    FROM dbo.t_Transaction
                    WHERE TransactionId = @TxId
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                );

                INSERT INTO dbo.t_AuditLog
                (
                    UserId, LoginId, EntityType, EntityId, Action,
                    beforeState, afterState, CreatedAt
                )
                VALUES
                (
                    @ReviewerId, @LoginId, 'Transaction', @TxId, 'UPDATE',
                    @TxBeforeState, @TxAfterState, SYSUTCDATETIME()
                );

                -- Notify initiator on approve
                IF @InitiatorUserId IS NOT NULL
                BEGIN
                    INSERT INTO dbo.t_Notification (UserId, [Message], [Status], [Type], CreatedDate, UpdatedAt)
                    VALUES
                    (
                        @InitiatorUserId,
                        N'Your transaction #' + CAST(@TxId AS NVARCHAR(20))
                        + N' (' + @TypeText + N') for amount '
                        + CONVERT(NVARCHAR(50), @Amount)
                        + N' has been ' + @DecisionText
                        + N' by reviewer #' + CAST(@ReviewerId AS NVARCHAR(20)) + N'.',
                        1, 4, SYSUTCDATETIME(), NULL
                    );
                END
            END
            ELSE IF @Decision = @DEC_REJECT
            BEGIN
                DECLARE @TypeR INT, @AmountR DECIMAL(18,2), @InitiatorUserIdR INT;

                -- For REJECT we only flip txn status; capture BEFORE/AFTER for audit
                DECLARE @TxBeforeR NVARCHAR(MAX), @TxAfterR NVARCHAR(MAX);

                SELECT
                    @TypeR            = T.[Type],
                    @AmountR          = T.Amount,
                    @InitiatorUserIdR = T.CreatedByUserId
                FROM dbo.t_Transaction AS T WITH (HOLDLOCK)
                WHERE T.TransactionId = @TxId;

                SELECT @TxBeforeR = (
                    SELECT *
                    FROM dbo.t_Transaction
                    WHERE TransactionId = @TxId
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                );

                UPDATE dbo.t_Transaction
                   SET [Status]  = @TXN_REJECTED,
                       UpdatedAt = SYSUTCDATETIME()
                 WHERE TransactionId = @TxId;

                SELECT @TxAfterR = (
                    SELECT *
                    FROM dbo.t_Transaction
                    WHERE TransactionId = @TxId
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                );

                INSERT INTO dbo.t_AuditLog
                (
                    UserId, LoginId, EntityType, EntityId, Action,
                    beforeState, afterState, CreatedAt
                )
                VALUES
                (
                    @ReviewerId, @LoginId, 'Transaction', @TxId, 'UPDATE',
                    @TxBeforeR, @TxAfterR, SYSUTCDATETIME()
                );

                DECLARE @TypeTextR NVARCHAR(20) =
                    CASE @TypeR WHEN 1 THEN N'Deposit'
                                WHEN 2 THEN N'Withdrawal'
                                WHEN 3 THEN N'Transfer' END;

                IF @InitiatorUserIdR IS NOT NULL
                BEGIN
                    INSERT INTO dbo.t_Notification (UserId, [Message], [Status], [Type], CreatedDate, UpdatedAt)
                    VALUES
                    (
                        @InitiatorUserIdR,
                        N'Your transaction #' + CAST(@TxId AS NVARCHAR(20))
                        + N' (' + ISNULL(@TypeTextR, N'Unknown') + N') for amount '
                        + CONVERT(NVARCHAR(50), @AmountR)
                        + N' has been ' + @DecisionText
                        + N' by reviewer #' + CAST(@ReviewerId AS NVARCHAR(20)) + N'.',
                        1, 4, SYSUTCDATETIME(), NULL
                    );
                END
            END

            COMMIT TRAN;

            SELECT 1 AS Success, NULL AS ErrorCode, N'Decision updated successfully' AS Message;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRAN;

            SELECT 0 AS Success,
                   'DB_ERROR' AS ErrorCode,
                   ERROR_MESSAGE() AS Message;
        END CATCH

        RETURN;
    END

    ------------------------------------------------------------
    -- ACTION: LIST (List approvals with filters + branch security)
    ------------------------------------------------------------
    IF UPPER(@Action) = 'LIST'
    BEGIN
        SET NOCOUNT ON;

        -- Safe sort whitelist
        DECLARE @SortColumn NVARCHAR(100) =
            CASE UPPER(ISNULL(@SortBy,'TRANSACTIONDATE'))
                WHEN 'TRANSACTIONDATE' THEN 'TransactionDate'
                WHEN 'AMOUNT'          THEN 'Amount'
                WHEN 'APPROVALID'      THEN 'ApprovalId'
                ELSE 'TransactionDate'
            END;

        DECLARE @SortDirection NVARCHAR(4) =
            CASE UPPER(ISNULL(@SortDir,'DESC'))
                WHEN 'ASC'  THEN 'ASC'
                WHEN 'DESC' THEN 'DESC'
                ELSE 'DESC'
            END;

        ;WITH Base AS
        (
            SELECT
                A.ApprovalId,
                A.TransactionId,
                A.Decision,
                A.Comments,
                T.FromAccountId AS AccountId,
                T.[Type],
                T.Amount,
                T.CreatedAt AS TransactionDate,
                A.ReviewerId,
                U.[Name] AS ReviewerName,
                CAST(U.[Role] AS INT) AS ReviewerRole,
                T.BranchId
            FROM dbo.t_Approval AS A
            JOIN dbo.t_Transaction AS T
              ON T.TransactionId = A.TransactionId
            JOIN dbo.t_User AS U
              ON U.UserId = A.ReviewerId
            WHERE
                (@AccountId  IS NULL OR T.FromAccountId = @AccountId OR T.ToAccountId = @AccountId)
                AND (@ReviewerIdF IS NULL OR A.ReviewerId = @ReviewerIdF)
                AND (@DecisionF  IS NULL OR  TRY_CONVERT(INT,A.Decision)   = @DecisionF)
                AND (@Type       IS NULL OR T.[Type]     = @Type)
                AND (@MinAmount  IS NULL OR T.Amount     >= @MinAmount)
                AND (@MaxAmount  IS NULL OR T.Amount     <= @MaxAmount)
                AND (@FromDate   IS NULL OR T.CreatedAt  >= @FromDate)
                AND (@ToDate     IS NULL OR T.CreatedAt  <  DATEADD(DAY, 1, @ToDate)) -- inclusive
                AND ( @UserRole = @ROLE_ADMIN OR T.BranchId = @UserBranchId )        -- branch security
        )
        SELECT
            b.ApprovalId,
            b.TransactionId,
            b.Decision,
            b.Comments,
            b.AccountId,
            b.[Type],
            b.Amount,
            b.TransactionDate,
            b.ReviewerId,
            b.ReviewerName,
            b.ReviewerRole,
            b.BranchId,
            COUNT(*) OVER() AS TotalCount
        FROM Base b
        ORDER BY
            CASE WHEN @SortColumn = 'TransactionDate' AND @SortDirection = 'ASC'  THEN b.TransactionDate END ASC,
            CASE WHEN @SortColumn = 'Amount'          AND @SortDirection = 'ASC'  THEN b.Amount END ASC,
            CASE WHEN @SortColumn = 'ApprovalId'      AND @SortDirection = 'ASC'  THEN b.ApprovalId END ASC,
            CASE WHEN @SortColumn = 'TransactionDate' AND @SortDirection = 'DESC' THEN b.TransactionDate END DESC,
            CASE WHEN @SortColumn = 'Amount'          AND @SortDirection = 'DESC' THEN b.Amount END DESC,
            CASE WHEN @SortColumn = 'ApprovalId'      AND @SortDirection = 'DESC' THEN b.ApprovalId END DESC
        OFFSET @Offset ROWS
        FETCH NEXT @Limit ROWS ONLY;

        RETURN;
    END
END";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            @"IF OBJECT_ID('[dbo].[usp_Approval]', 'P') IS NOT NULL
              DROP PROCEDURE [dbo].[usp_Approval];");
        }
    }
}
