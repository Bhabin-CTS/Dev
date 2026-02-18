using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Approval_usp_UpdateApprovalDecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.usp_UpdateApprovalDecision', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.usp_UpdateApprovalDecision AS BEGIN SET NOCOUNT ON; END');
");

            // 2) Now alter with your full body (NO GO statements inside)
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateApprovalDecision]
    @ApprovalID  int,
    @ReviewerID  int,
    @Decision    int,
    @Comments    nvarchar(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @DEC_PENDING    int = 1,
            @DEC_APPROVE    int = 2,
            @DEC_REJECT     int = 3;

    DECLARE @TXN_COMPLETED  int = 1,
            @TXN_PENDING    int = 2,
            @TXN_REJECTED   int = 3; -- assuming 3 is Rejected in your enum

    -- ===== Business validations return a result row (no THROW) =====
    IF NOT EXISTS (SELECT 1 FROM dbo.t_Approval WHERE ApprovalId = @ApprovalID)
    BEGIN
        SELECT 0 AS Success, 'APPROVAL_NOT_FOUND' AS ErrorCode, N'Approval not found.' AS Message;
        RETURN;
    END

    DECLARE
        @TxId int,
        @CurrentReviewerId int,
        @CurrentDecision int;

    SELECT
        @TxId = A.TransactionId,
        @CurrentReviewerId = A.ReviewerId,
        @CurrentDecision = A.Decision
    FROM dbo.t_Approval AS A
    WHERE A.ApprovalId = @ApprovalID;

    IF @CurrentReviewerId <> @ReviewerID
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

        ----------------------------------------------------------------
        -- 1) Update the approval row
        ----------------------------------------------------------------
        UPDATE dbo.t_Approval
          SET Decision  = @Decision,
              Comments  = @Comments,
              DecidedAt = CASE WHEN @Decision <> @DEC_PENDING THEN SYSUTCDATETIME() ELSE NULL END
        WHERE ApprovalId = @ApprovalID;

        ----------------------------------------------------------------
        -- Variables used for transaction processing + notification
        ----------------------------------------------------------------
        DECLARE
            @Type int,
            @Amount decimal(18,2),
            @FromAccountId int,
            @ToAccountId int,
            @TxnStatus int,
            @FromBal decimal(18,2),
            @ToBal decimal(18,2),
            @NewFromBal decimal(18,2),
            @NewToBal decimal(18,2),
            @InitiatorUserId int = NULL,
            @DecisionText nvarchar(20),
            @TypeText nvarchar(20);

        SET @DecisionText = CASE @Decision
                                WHEN @DEC_APPROVE THEN N'approved'
                                WHEN @DEC_REJECT  THEN N'rejected'
                                ELSE N'pending'
                            END;

        IF @Decision = @DEC_APPROVE
        BEGIN
            -- Lock transaction
            SELECT
                @Type            = T.[Type],
                @Amount          = T.Amount,
                @FromAccountId   = T.FromAccountId,
                @ToAccountId     = T.ToAccountId,
                @TxnStatus       = T.[Status],
                @InitiatorUserId = T.CreatedByUserId
            FROM dbo.t_Transaction AS T WITH (UPDLOCK, ROWLOCK)
            WHERE T.TransactionID = @TxId;

            IF @Type IS NULL
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

            -- Lock accounts and compute
            SELECT @FromBal = A.Balance
            FROM dbo.t_Account AS A WITH (UPDLOCK, ROWLOCK)
            WHERE A.AccountID = @FromAccountId;

            IF @FromBal IS NULL
            BEGIN
                ROLLBACK TRAN;
                SELECT 0 AS Success, 'FROM_NOT_FOUND' AS ErrorCode, N'From account not found.' AS Message;
                RETURN;
            END

            IF @Type = 3 /* Transfer */
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
                WHERE A.AccountID = @ToAccountId;

                IF @ToBal IS NULL
                BEGIN
                    ROLLBACK TRAN;
                    SELECT 0 AS Success, 'TO_NOT_FOUND' AS ErrorCode, N'To account not found.' AS Message;
                    RETURN;
                END
            END

            IF @Type = 1 /* Deposit */
            BEGIN
                SET @NewFromBal = @FromBal + @Amount;

                UPDATE dbo.t_Account
                  SET Balance = @NewFromBal
                WHERE AccountID = @FromAccountId;

                UPDATE dbo.t_Transaction
                  SET [Status]        = @TXN_COMPLETED,
                      BalanceAfterTxn = @NewFromBal,
                      UpdatedAt       = SYSUTCDATETIME()
                WHERE TransactionID = @TxId;

                SET @TypeText = N'Deposit';
            END
            ELSE IF @Type = 2 /* Withdrawal */
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
                WHERE AccountID = @FromAccountId;

                UPDATE dbo.t_Transaction
                  SET [Status]        = @TXN_COMPLETED,
                      BalanceAfterTxn = @NewFromBal,
                      UpdatedAt       = SYSUTCDATETIME()
                WHERE TransactionID = @TxId;

                SET @TypeText = N'Withdrawal';
            END
            ELSE IF @Type = 3 /* Transfer */
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
                WHERE AccountID = @FromAccountId;

                UPDATE dbo.t_Account
                  SET Balance = @NewToBal
                WHERE AccountID = @ToAccountId;

                UPDATE dbo.t_Transaction
                  SET [Status]        = @TXN_COMPLETED,
                      BalanceAfterTxn = @NewFromBal,
                      UpdatedAt       = SYSUTCDATETIME()
                WHERE TransactionID = @TxId;

                SET @TypeText = N'Transfer';
            END
            ELSE
            BEGIN
                ROLLBACK TRAN;
                SELECT 0 AS Success, 'UNKNOWN_TX_TYPE' AS ErrorCode, N'Unknown transaction type.' AS Message;
                RETURN;
            END

            -- Notification on approve
            IF @InitiatorUserId IS NOT NULL
            BEGIN
                INSERT INTO dbo.t_Notification
                    (UserId, [Message], [Status], [Type], CreatedDate, UpdatedAt)
                VALUES
                    (
                        @InitiatorUserId,
                        N'Your transaction #' + CAST(@TxId AS nvarchar(20))
                        + N' (' + @TypeText + N') for amount '
                        + CONVERT(nvarchar(50), @Amount)
                        + N' has been ' + @DecisionText
                        + N' by reviewer #' + CAST(@ReviewerID AS nvarchar(20)) + N'.',
                        1, 4, SYSUTCDATETIME(), NULL
                    );
            END
        END
        ELSE IF @Decision = @DEC_REJECT
        BEGIN
            DECLARE @TypeR int, @AmountR decimal(18,2), @InitiatorUserIdR int;

            SELECT
                @TypeR            = T.[Type],
                @AmountR          = T.Amount,
                @InitiatorUserIdR = T.CreatedByUserId
            FROM dbo.t_Transaction AS T WITH (HOLDLOCK)
            WHERE T.TransactionID = @TxId;

            UPDATE dbo.t_Transaction
               SET [Status]  = @TXN_REJECTED,
                   UpdatedAt = SYSUTCDATETIME()
             WHERE TransactionID = @TxId;

            DECLARE @TypeTextR nvarchar(20) =
                CASE @TypeR WHEN 1 THEN N'Deposit'
                            WHEN 2 THEN N'Withdrawal'
                            WHEN 3 THEN N'Transfer' END;

            IF @InitiatorUserIdR IS NOT NULL
            BEGIN
                INSERT INTO dbo.t_Notification
                    (UserId, [Message], [Status], [Type], CreatedDate, UpdatedAt)
                VALUES
                    (
                        @InitiatorUserIdR,
                        N'Your transaction #' + CAST(@TxId AS nvarchar(20))
                        + N' (' + ISNULL(@TypeTextR, N'Unknown') + N') for amount '
                        + CONVERT(nvarchar(50), @AmountR)
                        + N' has been ' + @DecisionText
                        + N' by reviewer #' + CAST(@ReviewerID AS nvarchar(20)) + N'.',
                        1, 4, SYSUTCDATETIME(), NULL
                    );
            END
        END

        COMMIT TRAN;

        -- Final success envelope
        SELECT 1 AS Success, NULL AS ErrorCode, N'Decision updated successfully' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        SELECT 0 AS Success,
               'DB_ERROR' AS ErrorCode,
               ERROR_MESSAGE() AS Message;
    END CATCH
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.usp_UpdateApprovalDecision', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_UpdateApprovalDecision;
");
        }
    }
}
