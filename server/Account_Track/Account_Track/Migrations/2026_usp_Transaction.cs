using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_Transaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_Transaction]
            (
                @Action VARCHAR(20),
 
                -- Common
                @UserId INT=NULL,
                @LoginId INT = NULL,
 
                -- Create
                @CreatedByUserId INT = NULL,
                @FromAccountId INT = NULL,
                @ToAccountId INT = NULL,
                @Type INT = NULL,
                @Amount DECIMAL(18,2) = NULL,
                @Remarks VARCHAR(500) = NULL,
 
                -- GetById
                @TransactionId INT = NULL,
 
                -- List Filters
                @AccountId INT = NULL,
                @Status INT = NULL,
                @IsHighValue BIT = NULL,
                @CreatedFrom DATE = NULL,
                @CreatedTo DATE = NULL,
                @UpdatedFrom DATE = NULL,
                @UpdatedTo DATE = NULL,
                @SortBy NVARCHAR(50) = 'CreatedAt',
                @SortOrder NVARCHAR(10) = 'DESC',
                @Limit INT = 20,
                @Offset INT = 0
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                ------------------------------------------------------------
                -- SHARED VARIABLE DECLARATIONS (used across multiple actions)
                ------------------------------------------------------------
                DECLARE @UserBranchId INT;
                DECLARE @UserRole INT;
                DECLARE @ROLE_ADMIN INT = 3;
                DECLARE @ROLE_MANAGER INT = 2;
                DECLARE @ROLE_OFFICER INT = 1;
 
                ------------------------------------------------------------
                -- CREATE TRANSACTION
                ------------------------------------------------------------
                IF @Action = 'CREATE'
                BEGIN
                    SET NOCOUNT ON;
 
                    BEGIN TRY

                        BEGIN TRANSACTION;
 
                        ----------------------------------------------------
                        -- ENUM CONSTANTS (KEEP IN SYNC WITH C#)
                        ----------------------------------------------------
                        DECLARE @STATUS_COMPLETED INT = 1;
                        DECLARE @STATUS_PENDING   INT = 2;
 
                        DECLARE @APPROVAL_PENDING INT = 1;
 
                        DECLARE @NOTIF_UNREAD INT = 1;
                        DECLARE @NOTIF_APPROVAL_REMINDER INT = 1;
 
                        DECLARE @ACCOUNT_STATUS_ACTIVE INT = 1;
                        DECLARE @ACCOUNT_STATUS_CLOSED INT = 2;

                        ----------------------------------------------------
                        -- DETERMINE HIGH VALUE AND STATUS
                        -- Note: Renamed to @TxnIsHighValue and @TxnStatus
                        --       to avoid conflict with input parameters
                        --       @IsHighValue and @Status
                        ----------------------------------------------------
                        DECLARE @TxnIsHighValue BIT =
                            CASE WHEN @Amount >= 10000 THEN 1 ELSE 0 END;
 
                        DECLARE @TxnStatus INT =
                            CASE WHEN @TxnIsHighValue = 1 THEN @STATUS_PENDING
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
                        -- FETCH BRANCH ID FROM USER
                        ----------------------------------------------------
                        DECLARE @BranchId INT;
                        SELECT @BranchId = BranchId
                        FROM t_User
                        WHERE UserId = @CreatedByUserId;
 
                        ----------------------------------------------------
                        -- VALIDATIONS
                        ----------------------------------------------------
                        IF @FromAccountId IS NOT NULL
                        BEGIN
                            IF NOT EXISTS (
                                SELECT 1
                                FROM t_Account
                                WHERE AccountId = @FromAccountId
                            )
                            BEGIN
                                THROW 50010, 'From Account does not exist', 1;
                            END
 
                            IF EXISTS (
                                SELECT 1
                                FROM t_Account
                                WHERE AccountId = @FromAccountId
                                AND Status = @ACCOUNT_STATUS_CLOSED
                            )
                            BEGIN
                                THROW 50011, 'From Account is closed', 1;
                            END
                        END

                        ----------------------------------------------------
                        -- VALIDATE TO ACCOUNT (ONLY FOR TRANSFER)
                        ----------------------------------------------------
                        IF @Type = 3
                        BEGIN
                            IF @ToAccountId IS NULL
                            BEGIN
                                THROW 50012, 'To Account is required for transfer', 1;
                            END
 
                            IF NOT EXISTS (
                                SELECT 1
                                FROM t_Account
                                WHERE AccountId = @ToAccountId
                            )
                            BEGIN
                                THROW 50013, 'To Account does not exist', 1;
                            END
 
                            IF EXISTS (
                                SELECT 1
                                FROM t_Account
                                WHERE AccountId = @ToAccountId
                                AND Status = @ACCOUNT_STATUS_CLOSED
                            )
                            BEGIN
                                THROW 50014, 'To Account is closed', 1;
                            END
                        END

                        IF @BranchId IS NULL
                        BEGIN
                            THROW 50002, 'Invalid User: Branch not found', 1;
                        END

                        IF @Type IN (2,3) AND @BalanceBefore < @Amount
                        BEGIN
                            THROW 50001, 'Insufficient balance for transaction', 1;
                        END
 
 
                        ----------------------------------------------------
                        -- CALCULATE BALANCE AFTER (ONLY IF COMPLETED)
                        ----------------------------------------------------
                        SET @BalanceAfter =
                            CASE
                                WHEN @TxnStatus = @STATUS_COMPLETED THEN
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
                            BranchId,
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
                            @BranchId,
                            @CreatedByUserId,
                            @FromAccountId,
                            @ToAccountId,
                            @Type,
                            @Amount,
                            @TxnStatus,
                            @TxnIsHighValue,
                            @BalanceBefore,
                            @BalanceAfter,
                            @Remarks,
                            GETUTCDATE()
                        );
 
                        DECLARE @NewTransactionId INT = SCOPE_IDENTITY();

                        -- AUDIT LOG FOR TRANSACTION CREATE
                        DECLARE @TxnAfterState NVARCHAR(MAX);
                        SELECT @TxnAfterState = (
                            SELECT * FROM t_Transaction
                            WHERE TransactionId = @NewTransactionId
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                        );

                        INSERT INTO t_AuditLog
                        (
                            UserId,
                            LoginId,
                            EntityType,
                            EntityId,
                            Action,
                            beforeState,
                            afterState,
                            CreatedAt
                        )
                        VALUES
                        (
                            @CreatedByUserId,
                            @LoginId,
                            'Transaction',
                            @NewTransactionId,
                            'CREATE',
                            NULL,
                            @TxnAfterState,
                            GETUTCDATE()
                        );
 
                        ----------------------------------------------------
                        -- HIGH VALUE LOGIC : APPROVAL + NOTIFICATION
                        ----------------------------------------------------
                        IF @TxnIsHighValue = 1
                        BEGIN

                            DECLARE @ManagerUserId INT;

                            -- Find active manager of same branch
                            SELECT TOP 1 @ManagerUserId = UserId
                            FROM t_User
                            WHERE BranchId = @BranchId
                              AND Role = @ROLE_MANAGER
                              AND Status = 1;
 
 
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

                            DECLARE @NewApprovalId INT = SCOPE_IDENTITY();

                            -- AUDIT LOG FOR APPROVAL CREATE
                            DECLARE @ApprovalAfterState NVARCHAR(MAX);
                            SELECT @ApprovalAfterState = (
                                SELECT * FROM t_Approval
                                WHERE ApprovalId = @NewApprovalId
                                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                            );

                            INSERT INTO t_AuditLog
                            (
                                UserId,
                                LoginId,
                                EntityType,
                                EntityId,
                                Action,
                                beforeState,
                                afterState,
                                CreatedAt
                            )
                            VALUES
                            (
                                @CreatedByUserId,
                                @LoginId,
                                'Approval',
                                @NewApprovalId,
                                'CREATE',
                                NULL,
                                @ApprovalAfterState,
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
                        IF @TxnStatus = @STATUS_COMPLETED
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

                        COMMIT TRANSACTION;
  
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
 
                        IF @@TRANCOUNT > 0
                            ROLLBACK TRANSACTION;

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
                    RETURN;
                END
 
                ------------------------------------------------------------
                -- GET BY ID
                ------------------------------------------------------------
                IF @Action = 'GET_BY_ID'
                BEGIN
                    SET NOCOUNT ON;

                    -- Get user info from DB
                    SELECT 
                        @UserBranchId = BranchId,
                        @UserRole = Role
                    FROM t_User
                    WHERE UserId = @UserId;

                    SELECT
                        TransactionId,
                        CreatedByUserId As CreatedBy,
                        Type,
                        Amount,
                        Status,
                        IsHighValue,
                        FromAccountId,
                        ToAccountId,
                        BalanceBefore,
                        BalanceAfterTxn as BalanceAfter,
                        CreatedAt,
                        UpdatedAt
                    FROM t_Transaction
                    WHERE TransactionId = @TransactionId
                    AND (
                            @UserRole = @ROLE_ADMIN
                            OR BranchId = @UserBranchId
                        );
 
                    RETURN;
                END
 
                ------------------------------------------------------------
                -- LIST
                ------------------------------------------------------------
                IF @Action = 'LIST'
                BEGIN
                    SET NOCOUNT ON;

                    SELECT 
                        @UserBranchId = BranchId,
                        @UserRole = Role
                    FROM t_User
                    WHERE UserId = @UserId;

                    SELECT
                        t.TransactionId,
                        t.Type,
                        t.Amount,
                        t.Status,
                        t.IsHighValue,
                        t.CreatedAt,
                        t.UpdatedAt,
                        COUNT(*) OVER() AS TotalCount
                    FROM t_Transaction t
                    WHERE
                        (@AccountId IS NULL OR t.FromAccountId = @AccountId OR t.ToAccountId = @AccountId)
                        AND (@Type IS NULL OR t.Type = @Type)
                        AND (@Status IS NULL OR t.Status = @Status)
                        AND (@IsHighValue IS NULL OR t.IsHighValue = @IsHighValue)
                        AND (@CreatedFrom IS NULL OR t.CreatedAt >= @CreatedFrom)
                        AND (@CreatedTo IS NULL OR t.CreatedAt <= @CreatedTo)
                        AND (@UpdatedFrom IS NULL OR t.UpdatedAt >= @UpdatedFrom)
                        AND (@UpdatedTo IS NULL OR t.UpdatedAt <= @UpdatedTo)
                        AND (
                            @UserRole = @ROLE_ADMIN
                            OR t.BranchId = @UserBranchId
                        )
                    ORDER BY
                        CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'ASC' THEN t.CreatedAt END ASC,
                        CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'DESC' THEN t.CreatedAt END DESC,

                        CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'ASC' THEN t.UpdatedAt END ASC,
                        CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'DESC' THEN t.UpdatedAt END DESC,

                        CASE WHEN @SortBy = 'Amount' AND @SortOrder = 'ASC' THEN t.Amount END ASC,
                        CASE WHEN @SortBy = 'Amount' AND @SortOrder = 'DESC' THEN t.Amount END DESC,

                        CASE WHEN @SortBy = 'Type' AND @SortOrder = 'ASC' THEN t.Type END ASC,
                        CASE WHEN @SortBy = 'Type' AND @SortOrder = 'DESC' THEN t.Type END DESC,

                        CASE WHEN @SortBy = 'Status' AND @SortOrder = 'ASC' THEN t.Status END ASC,
                        CASE WHEN @SortBy = 'Status' AND @SortOrder = 'DESC' THEN t.Status END DESC,

                        CASE WHEN @SortBy = 'IsHighValue' AND @SortOrder = 'ASC' THEN t.IsHighValue END ASC,
                        CASE WHEN @SortBy = 'IsHighValue' AND @SortOrder = 'DESC' THEN t.IsHighValue END DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @Limit ROWS ONLY;
                    RETURN;
                END
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            @"IF OBJECT_ID('[dbo].[usp_Transaction]', 'P') IS NOT NULL
              DROP PROCEDURE [dbo].[usp_Transaction];");
        }
    }
}