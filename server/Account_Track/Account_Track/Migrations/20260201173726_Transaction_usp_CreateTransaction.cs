using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Transaction_usp_CreateTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_CreateTransaction = @"
                CREATE PROCEDURE [dbo].[usp_CreateTransaction]
                (
                    @CreatedByUserId INT,
                    @FromAccountId INT = NULL,
                    @ToAccountId INT = NULL,
                    @Type INT,                -- 1=Deposit, 2=Withdrawal, 3=Transfer
                    @Amount DECIMAL(18,2),
                    @Remarks VARCHAR(500) = NULL
                )
                AS
                BEGIN
                    SET NOCOUNT ON;
 
                    BEGIN TRY
 
                        ----------------------------------------------------
                        -- ENUM CONSTANTS (KEEP IN SYNC WITH C#)
                        ----------------------------------------------------
                        DECLARE @STATUS_COMPLETED INT = 1;
                        DECLARE @STATUS_PENDING   INT = 2;
 
                        DECLARE @ROLE_MANAGER INT = 2;
 
                        DECLARE @APPROVAL_PENDING INT = 1;
 
                        DECLARE @NOTIF_UNREAD INT = 1;
                        DECLARE @NOTIF_APPROVAL_REMINDER INT = 1;
 
 
                        ----------------------------------------------------
                        -- DETERMINE HIGH VALUE AND STATUS
                        ----------------------------------------------------
                        DECLARE @IsHighValue BIT =
                            CASE WHEN @Amount >= 10000 THEN 1 ELSE 0 END;
 
                        DECLARE @Status INT =
                            CASE WHEN @IsHighValue = 1 THEN @STATUS_PENDING
                                 ELSE @STATUS_COMPLETED END;
 
 
                        ----------------------------------------------------
                        -- BALANCE VARIABLES
                        ----------------------------------------------------
                        DECLARE @BalanceBefore DECIMAL(18,2);
                        DECLARE @BalanceAfter DECIMAL(18,2);
 
 
                        ----------------------------------------------------
                        -- FETCH BALANCE BEFORE TRANSACTION
                        ----------------------------------------------------
                        SELECT @BalanceBefore = Balance
                        FROM t_Account
                        WHERE AccountId = @FromAccountId;
 
 
                        ----------------------------------------------------
                        -- VALIDATIONS
                        ----------------------------------------------------
                        IF @Type IN (2,3) AND @BalanceBefore < @Amount
                        BEGIN
                            THROW 50001, 'Insufficient balance for transaction', 1;
                        END
 
 
                        ----------------------------------------------------
                        -- CALCULATE BALANCE AFTER (ONLY IF COMPLETED)
                        ----------------------------------------------------
                        SET @BalanceAfter =
                            CASE
                                WHEN @Status = @STATUS_COMPLETED THEN
                                    CASE
                                        WHEN @Type = 1 THEN @BalanceBefore + @Amount
                                        WHEN @Type IN (2,3) THEN @BalanceBefore - @Amount
                                    END
                                ELSE NULL
                            END;
 
 
                        ----------------------------------------------------
                        -- INSERT TRANSACTION RECORD
                        ----------------------------------------------------
                        INSERT INTO t_Transaction
                        (
                            CreatedByUserId,
                            FromAccountId,
                            ToAccountId,
                            Type,
                            Amount,
                            Status,
                            IsHighValue,
                            BalanceBefore,
                            BalanceAfterTxn,
                            flagReason,
                            CreatedAt
                        )
                        VALUES
                        (
                            @CreatedByUserId,
                            @FromAccountId,
                            @ToAccountId,
                            @Type,
                            @Amount,
                            @Status,
                            @IsHighValue,
                            @BalanceBefore,
                            @BalanceAfter,
                            @Remarks,
                            GETUTCDATE()
                        );
 
                        DECLARE @NewTransactionId INT = SCOPE_IDENTITY();
 
 
                        ----------------------------------------------------
                        -- HIGH VALUE LOGIC : APPROVAL + NOTIFICATION
                        ----------------------------------------------------
                        IF @IsHighValue = 1
                        BEGIN
 
                            DECLARE @BranchId INT;
                            DECLARE @ManagerUserId INT;
 
 
                            -- Get branch of the officer who created transaction
                            SELECT @BranchId = BranchId
                            FROM t_User
                            WHERE UserId = @CreatedByUserId;
 
 
                            -- Find active manager of same branch
                            SELECT TOP 1 @ManagerUserId = UserId
                            FROM t_User
                            WHERE BranchId = @BranchId
                              AND Role = @ROLE_MANAGER
                              AND Status = 1;      -- assuming 1 = Active user status
 
 
                            ------------------------------------------------
                            -- CREATE APPROVAL ENTRY
                            ------------------------------------------------
                            INSERT INTO t_Approval
                            (
                                TransactionId,
                                ReviewerId,
                                Decision,
                                Comments,
                                CreatedAt
                            )
                            VALUES
                            (
                                @NewTransactionId,
                                @ManagerUserId,
                                @APPROVAL_PENDING,
                                NULL,
                                GETUTCDATE()
                            );
 
 
                            ------------------------------------------------
                            -- CREATE NOTIFICATION FOR MANAGER
                            ------------------------------------------------
                            INSERT INTO t_Notification
                            (
                                UserId,
                                Message,
                                Status,
                                Type,
                                CreatedDate
                            )
                            VALUES
                            (
                                @ManagerUserId,
                                CONCAT('Approval required for Transaction #', @NewTransactionId),
                                @NOTIF_UNREAD,
                                @NOTIF_APPROVAL_REMINDER,
                                GETUTCDATE()
                            );
 
                        END
 
 
                        ----------------------------------------------------
                        -- UPDATE ACCOUNT BALANCES (ONLY IF COMPLETED)
                        ----------------------------------------------------
                        IF @Status = @STATUS_COMPLETED
                        BEGIN
 
                            -- Deposit
                            IF @Type = 1
                            BEGIN
                                UPDATE t_Account
                                SET Balance = Balance + @Amount
                                WHERE AccountId = @FromAccountId;
                            END
 
                            -- Withdrawal
                            ELSE IF @Type = 2
                            BEGIN
                                UPDATE t_Account
                                SET Balance = Balance - @Amount
                                WHERE AccountId = @FromAccountId;
                            END
 
                            -- Transfer
                            ELSE IF @Type = 3
                            BEGIN
                                -- Deduct from source
                                UPDATE t_Account
                                SET Balance = Balance - @Amount
                                WHERE AccountId = @FromAccountId;
 
                                -- Credit to destination
                                UPDATE t_Account
                                SET Balance = Balance + @Amount
                                WHERE AccountId = @ToAccountId;
                            END
 
                        END
 
 
                        ----------------------------------------------------
                        -- RETURN RESULT TO API
                        ----------------------------------------------------
                        SELECT
                            1 AS Success,
                            NULL AS ErrorCode,
                            'Transaction created successfully' AS Message,
 
                            t.TransactionId,
                            t.Status,
                            t.Type,
                            CAST(t.IsHighValue AS BIT) AS IsHighValue,
                            CAST(CASE WHEN t.Status = @STATUS_PENDING THEN 1 ELSE 0 END AS BIT) AS ApprovalRequired,
                            t.CreatedAt
                        FROM t_Transaction t
                        WHERE t.TransactionId = @NewTransactionId;
 
 
                    END TRY
 
                    BEGIN CATCH
 
                        SELECT
                            0 AS Success,
                            'DB_ERROR' AS ErrorCode,
                            ERROR_MESSAGE() AS Message,
                            NULL AS TransactionId,
                            NULL AS Status,
                            NULL AS Type,
                            NULL AS IsHighValue,
                            NULL AS ApprovalRequired,
                            NULL AS CreatedAt;
 
                    END CATCH
 
                END;
                ";

            migrationBuilder.Sql(sp_CreateTransaction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DROP PROCEDURE IF EXISTS [dbo].[usp_CreateTransaction]"
            );
        }
    }
}
