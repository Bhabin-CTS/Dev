using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class _2026_usp_Account : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
CREATE OR ALTER PROCEDURE [dbo].[usp_Account]
(
    @Action VARCHAR(20),

    -- Common (mirrors usp_Transaction style)
    @UserId INT = NULL,
    @LoginId INT = NULL,

    -- Create
    @PerformedByUserId INT = NULL,
    @CustomerName NVARCHAR(100) = NULL,
    @AccountType INT = NULL,
    @InitialDeposit DECIMAL(18,2) = NULL,
    @Remarks NVARCHAR(500) = NULL,

    -- Update / GetById
    @AccountId INT = NULL,
    @RowVersionBase64 NVARCHAR(200) = NULL,

    -- List
    @Status INT = NULL,
    @SearchTerm NVARCHAR(100) = NULL,
    @CreatedFrom DATE = NULL,
    @CreatedTo DATE = NULL,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortOrder NVARCHAR(10) = 'DESC',
    @Limit INT = 20,
    @Offset INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Safety net so audit insert does not fail when LoginId is not supplied
    SET @LoginId = COALESCE(@LoginId, @UserId);

    ------------------------------------------------------------
    -- SHARED CONSTANTS (aligned with your enums/usage)
    ------------------------------------------------------------
    DECLARE @ROLE_ADMIN   INT = 3;
    DECLARE @ROLE_MANAGER INT = 2;
    DECLARE @NOTIF_UNREAD INT = 1;
    -- Officer = 1 (used directly)

    ------------------------------------------------------------
    -- CREATE
    ------------------------------------------------------------
    IF @Action = 'CREATE'
    BEGIN
        BEGIN TRY
            IF (@CustomerName IS NULL OR LTRIM(RTRIM(@CustomerName)) = '')
            BEGIN
                SELECT 0 AS Success, 'INVALID_REQUEST' AS ErrorCode, 'customerName is required' AS Message,
                       NULL AS AccountId, NULL AS AccountNumber, NULL AS AccountType, NULL AS Status,
                       NULL AS Balance, NULL AS CreatedAt;
                RETURN;
            END

            IF (@InitialDeposit IS NULL OR @InitialDeposit < 0)
            BEGIN
                SELECT 0 AS Success, 'INVALID_AMOUNT' AS ErrorCode, 'initialDeposit must be >= 0' AS Message,
                       NULL AS AccountId, NULL AS AccountNumber, NULL AS AccountType, NULL AS Status,
                       NULL AS Balance, NULL AS CreatedAt;
                RETURN;
            END

            DECLARE @BranchId INT, @Now DATETIME2 = SYSUTCDATETIME();
            SELECT @BranchId = u.BranchId
            FROM dbo.t_User u WITH (NOLOCK)
            WHERE u.UserId = @PerformedByUserId;

            IF (@BranchId IS NULL)
            BEGIN
                SELECT 0 AS Success, 'USER_NOT_FOUND' AS ErrorCode, 'PerformedBy user invalid' AS Message,
                       NULL AS AccountId, NULL AS AccountNumber, NULL AS AccountType, NULL AS Status,
                       NULL AS Balance, NULL AS CreatedAt;
                RETURN;
            END

            -- Generate unique AccountNumber with serialization to avoid duplicates
            DECLARE @AccountNumber INT;
            ;WITH mx AS (
                SELECT MAX(a.AccountNumber) AS MaxAcc
                FROM dbo.t_Account a WITH (UPDLOCK, HOLDLOCK)
            )
            SELECT @AccountNumber = ISNULL(MaxAcc, 1000000) + 1 FROM mx;

            DECLARE @AccountId_New INT;
            INSERT INTO dbo.t_Account
            (
                CustomerName, AccountNumber, BranchId, AccountType, Balance, Status,
                CreatedByUserId, CreatedAt, UpdatedAt
            )
            VALUES
            (
                @CustomerName, @AccountNumber, @BranchId, @AccountType,
                @InitialDeposit, 1,   -- Active
                @PerformedByUserId, @Now, NULL
            );

            SET @AccountId_New = SCOPE_IDENTITY();

            DECLARE @ManagerUserId INT;
            SELECT TOP (1) @ManagerUserId = u.UserId
            FROM dbo.t_User u WITH (NOLOCK)
            WHERE u.BranchId = @BranchId
              AND u.Role     = @ROLE_MANAGER
              AND u.Status   = 1;

            IF @ManagerUserId IS NOT NULL
            BEGIN
                BEGIN TRY
                    INSERT INTO dbo.t_Notification
                    (
                        UserId, Message, Status, Type, CreatedDate
                    )
                    VALUES
                    (
                        @ManagerUserId,
                        CONCAT('New account #', @AccountNumber, ' created for ', @CustomerName, ' (AccountId ', @AccountId_New, ').'),
                        @NOTIF_UNREAD,
                        3,
                        GETUTCDATE()
                    );
                END TRY
                BEGIN CATCH
                    -- swallow notification errors
                END CATCH
            END

            ----------------------------------------------------------------
            -- AUDIT LOG (CREATE) - ONLY ADDITION
            ----------------------------------------------------------------
            BEGIN TRY
                DECLARE @CustomerNameEsc NVARCHAR(MAX) = REPLACE(ISNULL(@CustomerName, N''), N'""', N'""""');
                DECLARE @RemarksEsc NVARCHAR(MAX) = CASE WHEN @Remarks IS NOT NULL THEN REPLACE(@Remarks, N'""', N'""""') END;

                DECLARE @AfterJson_Create NVARCHAR(MAX) =
                    CONCAT(
                        N'{',
                        N'""AccountId"":',       @AccountId_New, N',',
                        N'""AccountNumber"":',   @AccountNumber, N',',
                        N'""CustomerName"":',    N'""', @CustomerNameEsc, N'""', N',',
                        N'""AccountType"":',     COALESCE(CAST(@AccountType AS NVARCHAR(20)), N'null'), N',',
                        N'""Status"":',          N'1', N',',
                        N'""Balance"":',         COALESCE(CAST(@InitialDeposit AS NVARCHAR(50)), N'0'), N',',
                        N'""BranchId"":',        @BranchId, N',',
                        N'""CreatedByUserId"":', @PerformedByUserId,
                        CASE WHEN @Remarks IS NOT NULL THEN CONCAT(N',', N'""Remarks"":', N'""', @RemarksEsc, N'""') ELSE N'' END,
                        N'}'
                    );

                INSERT INTO dbo.t_AuditLog
                (
                    UserId, LoginId, EntityType, EntityId, Action,
                    beforeState, afterState, CreatedAt
                )
                VALUES
                (
                    @PerformedByUserId, @LoginId, N'Account', @AccountId_New, N'CREATE',
                    NULL, @AfterJson_Create, SYSUTCDATETIME()
                );
            END TRY
            BEGIN CATCH
                -- swallow audit errors
            END CATCH
            ----------------------------------------------------------------

            SELECT
                1 AS Success,
                NULL AS ErrorCode,
                'Account created successfully' AS Message,
                @AccountId_New AS AccountId,
                @AccountNumber AS AccountNumber,
                @AccountType AS AccountType,
                1 AS Status,
                CAST(@InitialDeposit AS DECIMAL(18,2)) AS Balance,
                @Now AS CreatedAt;
        END TRY
        BEGIN CATCH
            SELECT 0 AS Success,
                   'DB_ERROR' AS ErrorCode,
                   ERROR_MESSAGE() AS Message,
                   NULL AS AccountId, NULL AS AccountNumber, NULL AS AccountType, NULL AS Status,
                   NULL AS Balance, NULL AS CreatedAt;
        END CATCH
        RETURN;
    END

    ------------------------------------------------------------
    -- UPDATE (Officer-only Manager notification on real changes)
    ------------------------------------------------------------
    IF @Action = 'UPDATE'
    BEGIN
        SET NOCOUNT ON;

        IF (@RowVersionBase64 IS NULL OR LTRIM(RTRIM(@RowVersionBase64)) = '')
        BEGIN
            SELECT 0 AS Success, 'INVALID_REQUEST' AS ErrorCode, 'rowVersionBase64 is required' AS Message,
                   NULL AS AccountId, NULL AS AccountNumber, NULL AS CustomerName, NULL AS AccountType,
                   NULL AS Status, NULL AS Balance, NULL AS BranchId, NULL AS BranchName, NULL AS CreatedByUserId,
                   NULL AS CreatedAt, NULL AS UpdatedAt, NULL AS RowVersionBase64;
            RETURN;
        END

        -- validate account type (1=Savings, 2=Current)
        IF (@AccountType IS NOT NULL AND @AccountType NOT IN (1,2))
        BEGIN
            SELECT 0 AS Success, 'INVALID_REQUEST' AS ErrorCode, 'accountType must be 1 (Savings) or 2 (Current)' AS Message,
                   NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL;
            RETURN;
        END

        DECLARE @ExpectedRowVersion VARBINARY(8);
        SELECT @ExpectedRowVersion =
            CAST(CAST(N'' as xml).value('xs:base64Binary(sql:variable(""@RowVersionBase64""))','varbinary(max)') AS varbinary(8));

        DECLARE @UserBranchId_UPD INT, @UserRole_UPD INT, @IsAdmin_UPD BIT = 0;
        SELECT @UserBranchId_UPD = u.BranchId, @UserRole_UPD = u.Role
        FROM dbo.t_User u WITH (NOLOCK)
        WHERE u.UserId = @PerformedByUserId;

        IF (@UserRole_UPD = @ROLE_ADMIN) SET @IsAdmin_UPD = 1; -- Admin=3
        DECLARE @Now_UPD DATETIME2 = SYSUTCDATETIME();

        -- Access check
        IF EXISTS (
            SELECT 1 FROM dbo.t_Account a
            WHERE a.AccountId = @AccountId
              AND (@IsAdmin_UPD = 1 OR a.BranchId = @UserBranchId_UPD)
        )
        BEGIN
            -- Capture before/after to detect changes
            DECLARE @chg TABLE
            (
                OldName NVARCHAR(100), NewName NVARCHAR(100),
                OldStatus INT, NewStatus INT,
                OldType INT, NewType INT,
                AccountId INT, AccountNumber INT, BranchId INT
            );

            UPDATE a
               SET a.CustomerName = COALESCE(@CustomerName, a.CustomerName),
                   a.Status       = COALESCE(@Status, a.Status),
                   a.AccountType  = COALESCE(@AccountType, a.AccountType),
                   a.UpdatedAt    = @Now_UPD
            OUTPUT
                   deleted.CustomerName,
                   inserted.CustomerName,
                   deleted.Status,
                   inserted.Status,
                   deleted.AccountType,
                   inserted.AccountType,
                   inserted.AccountId,
                   inserted.AccountNumber,
                   inserted.BranchId
            INTO @chg
            FROM dbo.t_Account a
            WHERE a.AccountId = @AccountId
              AND a.RowVersion = @ExpectedRowVersion;

            IF (@@ROWCOUNT = 0)
            BEGIN
                SELECT 0 AS Success, 'CONFLICT' AS ErrorCode, 'Record was modified by someone else' AS Message,
                       NULL AS AccountId, NULL AS AccountNumber, NULL AS CustomerName, NULL AS AccountType,
                       NULL AS Status, NULL AS Balance, NULL AS BranchId, NULL AS BranchName, NULL AS CreatedByUserId,
                       NULL AS CreatedAt, NULL AS UpdatedAt, NULL AS RowVersionBase64;
                RETURN;
            END

            -- Derive change flags
            DECLARE
                @OldName NVARCHAR(100), @NewName NVARCHAR(100),
                @OldStatus INT, @NewStatus INT,
                @OldType INT, @NewType INT,
                @UpdAccId INT, @UpdAccNo INT, @UpdBranchId INT;

            SELECT TOP (1)
                @OldName = OldName,     @NewName = NewName,
                @OldStatus = OldStatus, @NewStatus = NewStatus,
                @OldType = OldType,     @NewType = NewType,
                @UpdAccId = AccountId,  @UpdAccNo = AccountNumber, @UpdBranchId = BranchId
            FROM @chg;

            DECLARE @NameChanged   BIT = CASE WHEN ISNULL(@OldName,'') <> ISNULL(@NewName,'') THEN 1 ELSE 0 END;
            DECLARE @StatusChanged BIT = CASE WHEN @OldStatus <> @NewStatus THEN 1 ELSE 0 END;
            DECLARE @TypeChanged   BIT = CASE WHEN @OldType <> @NewType THEN 1 ELSE 0 END;
            DECLARE @DidChange     BIT = CASE WHEN @NameChanged = 1 OR @StatusChanged = 1 OR @TypeChanged = 1 THEN 1 ELSE 0 END;

            -- Notify Manager only when updater is Officer and tracked fields changed
            IF (@UserRole_UPD = 1 AND @DidChange = 1)
            BEGIN
                DECLARE @ChangedList NVARCHAR(100) = '';
                IF (@NameChanged   = 1) SET @ChangedList = CONCAT(@ChangedList, CASE WHEN @ChangedList = '' THEN '' ELSE ', ' END, 'Name');
                IF (@StatusChanged = 1) SET @ChangedList = CONCAT(@ChangedList, CASE WHEN @ChangedList = '' THEN '' ELSE ', ' END, 'Status');
                IF (@TypeChanged   = 1) SET @ChangedList = CONCAT(@ChangedList, CASE WHEN @ChangedList = '' THEN '' ELSE ', ' END, 'Type');

                DECLARE @ManagerUserId_UPD INT;
                SELECT TOP (1) @ManagerUserId_UPD = u.UserId
                FROM dbo.t_User u WITH (NOLOCK)
                WHERE u.BranchId = @UpdBranchId
                  AND u.Role     = @ROLE_MANAGER
                  AND u.Status   = 1;

                IF (@ManagerUserId_UPD IS NOT NULL)
                BEGIN
                    BEGIN TRY
                        INSERT INTO dbo.t_Notification
                        (
                            UserId, Message, Status, Type, CreatedDate
                        )
                        VALUES
                        (
                            @ManagerUserId_UPD,
                            CONCAT('Account #', @UpdAccNo, ' was updated (AccountId ', @UpdAccId, '). Changed: ', @ChangedList),
                            @NOTIF_UNREAD,
                            3,
                            GETUTCDATE()
                        );
                    END TRY
                    BEGIN CATCH
                        -- swallow notification errors
                    END CATCH
                END
            END

            ------------------------------------------------------------
            -- AUDIT LOG (UPDATE) - ONLY ADDITION
            ------------------------------------------------------------
            IF (@DidChange = 1)
            BEGIN
                BEGIN TRY
                    DECLARE @Before NVARCHAR(MAX) = N'{';
                    DECLARE @After  NVARCHAR(MAX) = N'{';
                    DECLARE @sep NVARCHAR(2) = N'';

                    -- escape strings for JSON
                    DECLARE @OldNameEsc NVARCHAR(MAX) = REPLACE(ISNULL(@OldName, N''), N'""', N'""""');
                    DECLARE @NewNameEsc NVARCHAR(MAX) = REPLACE(ISNULL(@NewName, N''), N'""', N'""""');

                    IF (@NameChanged = 1)
                    BEGIN
                        SET @Before = CONCAT(@Before, @sep, N'""CustomerName"":', N'""', @OldNameEsc, N'""');
                        SET @After  = CONCAT(@After,  @sep, N'""CustomerName"":', N'""', @NewNameEsc, N'""');
                        SET @sep = N',';
                    END

                    IF (@StatusChanged = 1)
                    BEGIN
                        SET @Before = CONCAT(@Before, @sep, N'""Status"":', ISNULL(CAST(@OldStatus AS NVARCHAR(20)), N'null'));
                        SET @After  = CONCAT(@After,  @sep, N'""Status"":', ISNULL(CAST(@NewStatus AS NVARCHAR(20)), N'null'));
                        SET @sep = N',';
                    END

                    IF (@TypeChanged = 1)
                    BEGIN
                        SET @Before = CONCAT(@Before, @sep, N'""AccountType"":', ISNULL(CAST(@OldType AS NVARCHAR(20)), N'null'));
                        SET @After  = CONCAT(@After,  @sep, N'""AccountType"":', ISNULL(CAST(@NewType AS NVARCHAR(20)), N'null'));
                        SET @sep = N',';
                    END

                    SET @Before = CONCAT(@Before, N'}');
                    SET @After  = CONCAT(@After,  N'}');

                    INSERT INTO dbo.t_AuditLog
                    (
                        UserId, LoginId, EntityType, EntityId, Action,
                        beforeState, afterState, CreatedAt
                    )
                    VALUES
                    (
                        @PerformedByUserId, @LoginId, N'Account', @UpdAccId, N'UPDATE',
                        @Before, @After, SYSUTCDATETIME()
                    );
                END TRY
                BEGIN CATCH
                    -- swallow audit errors
                END CATCH
            END
            ------------------------------------------------------------

            -- Return full snapshot incl. branch fields and createdBy
            SELECT TOP (1)
                1 AS Success,
                NULL AS ErrorCode,
                'Account updated successfully' AS Message,
                a.AccountId,
                a.AccountNumber,
                a.CustomerName,
                a.AccountType,
                a.Status,
                a.Balance,
                a.BranchId,
                b.BranchName,
                a.CreatedByUserId,
                a.CreatedAt,
                a.UpdatedAt,
                CAST(N'' as xml).value('xs:base64Binary(sql:column(""a.RowVersion""))','NVARCHAR(100)') AS RowVersionBase64
            FROM dbo.t_Account a WITH (NOLOCK)
            INNER JOIN dbo.t_Branch b WITH (NOLOCK) ON b.BranchId = a.BranchId
            WHERE a.AccountId = @AccountId;
        END
        ELSE
        BEGIN
            SELECT 0 AS Success, 'ACCOUNT_NOT_FOUND_OR_ACCESS_DENIED' AS ErrorCode, 'Either not found or access denied' AS Message,
                   NULL AS AccountId, NULL AS AccountNumber, NULL AS CustomerName, NULL AS AccountType,
                   NULL AS Status, NULL AS Balance, NULL AS BranchId, NULL AS BranchName, NULL AS CreatedByUserId,
                   NULL AS CreatedAt, NULL AS UpdatedAt, NULL AS RowVersionBase64;
        END
        RETURN;
    END

    ------------------------------------------------------------
    -- GET BY ID
    ------------------------------------------------------------
    IF @Action = 'GET_BY_ID'
    BEGIN
        SET NOCOUNT ON;

        DECLARE 
            @UserBranchId_GBI INT,
            @UserRole_GBI INT,
            @IsAdmin_GBI BIT = 0;

        SELECT 
            @UserBranchId_GBI = u.BranchId, 
            @UserRole_GBI = u.Role
        FROM dbo.t_User AS u WITH (NOLOCK)
        WHERE u.UserId = @UserId;

        -- Admin = 3
        IF (@UserRole_GBI = @ROLE_ADMIN)
            SET @IsAdmin_GBI = 1;

        SELECT TOP (1)
            a.AccountId,
            a.AccountNumber,
            a.CustomerName,
            a.AccountType,
            a.Status,
            a.Balance,
            a.BranchId,
            b.BranchName,
            a.CreatedByUserId,
            a.CreatedAt,
            a.UpdatedAt,
            CAST(N'' as xml).value('xs:base64Binary(sql:column(""a.RowVersion""))', 'NVARCHAR(100)') AS RowVersionBase64
        FROM dbo.t_Account AS a WITH (NOLOCK)
        INNER JOIN dbo.t_Branch AS b WITH (NOLOCK) 
            ON b.BranchId = a.BranchId
        WHERE a.AccountId = @AccountId
          AND (@IsAdmin_GBI = 1 OR a.BranchId = @UserBranchId_GBI);
        RETURN;
    END

    ------------------------------------------------------------
    -- LIST
    ------------------------------------------------------------
    IF @Action = 'LIST'
    BEGIN
        SET NOCOUNT ON;

        DECLARE 
            @UserBranchId_LIST INT,
            @UserRole_LIST INT;

        SELECT 
            @UserBranchId_LIST = BranchId,
            @UserRole_LIST = Role
        FROM t_User
        WHERE UserId = @UserId;

        SELECT
            a.AccountId,
            a.AccountNumber,
            a.CustomerName,
            a.AccountType,
            a.Status,
            a.Balance,
            a.BranchId,
            b.BranchName,
            a.CreatedByUserId,
            a.CreatedAt,
            a.UpdatedAt,
            COUNT(*) OVER() AS TotalCount
        FROM t_Account a
        INNER JOIN t_Branch b ON b.BranchId = a.BranchId
        WHERE
            (@Status IS NULL OR a.Status = @Status) AND
            (@AccountType IS NULL OR a.AccountType = @AccountType) AND
            (@CreatedFrom IS NULL OR a.CreatedAt >= @CreatedFrom) AND
            (@CreatedTo   IS NULL OR a.CreatedAt <= @CreatedTo) AND
            (
                @SearchTerm IS NULL
                OR a.CustomerName LIKE '%' + @SearchTerm + '%'
                OR (TRY_CAST(@SearchTerm AS INT) IS NOT NULL AND a.AccountNumber = TRY_CAST(@SearchTerm AS INT))
            ) AND
            (
                @UserRole_LIST = @ROLE_ADMIN
                OR a.BranchId = @UserBranchId_LIST
            )
        ORDER BY
            CASE WHEN @SortBy = 'CreatedAt'     AND @SortOrder = 'ASC'  THEN a.CreatedAt     END ASC,
            CASE WHEN @SortBy = 'CreatedAt'     AND @SortOrder = 'DESC' THEN a.CreatedAt     END DESC,
            CASE WHEN @SortBy = 'UpdatedAt'     AND @SortOrder = 'ASC'  THEN a.UpdatedAt     END ASC,
            CASE WHEN @SortBy = 'UpdatedAt'     AND @SortOrder = 'DESC' THEN a.UpdatedAt     END DESC,
            CASE WHEN @SortBy = 'AccountNumber' AND @SortOrder = 'ASC'  THEN a.AccountNumber END ASC,
            CASE WHEN @SortBy = 'AccountNumber' AND @SortOrder = 'DESC' THEN a.AccountNumber END DESC,
            CASE WHEN @SortBy = 'CustomerName'  AND @SortOrder = 'ASC'  THEN a.CustomerName  END ASC,
            CASE WHEN @SortBy = 'CustomerName'  AND @SortOrder = 'DESC' THEN a.CustomerName  END DESC,
            CASE WHEN @SortBy = 'Balance'       AND @SortOrder = 'ASC'  THEN a.Balance       END ASC,
            CASE WHEN @SortBy = 'Balance'       AND @SortOrder = 'DESC' THEN a.Balance       END DESC,
            CASE WHEN @SortBy = 'AccountType'   AND @SortOrder = 'ASC'  THEN a.AccountType   END ASC,
            CASE WHEN @SortBy = 'AccountType'   AND @SortOrder = 'DESC' THEN a.AccountType   END DESC,
            CASE WHEN @SortBy = 'Status'        AND @SortOrder = 'ASC'  THEN a.Status        END ASC,
            CASE WHEN @SortBy = 'Status'        AND @SortOrder = 'DESC' THEN a.Status        END DESC
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
            migrationBuilder.Sql(@"
IF OBJECT_ID('[dbo].[usp_Account]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Account];
");
        }
    }
}