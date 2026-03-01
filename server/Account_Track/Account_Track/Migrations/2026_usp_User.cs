using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_User : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_User]
            (
                @Action VARCHAR(30),

                -- Common

                @UserId INT = NULL,
                @LoginId INT = NULL,
                @PerformedBy INT = NULL,

                -- Create / Update
                @Name NVARCHAR(100) = NULL,
                @Email NVARCHAR(150) = NULL,
                @Role INT = NULL,
                @BranchId INT = NULL,
                @PasswordHash NVARCHAR(200) = NULL,

                -- Status Update
                @Status INT = NULL,
                @IsLocked BIT = NULL,

                -- List Filters
                @Search NVARCHAR(100) = NULL,
                @CreatedFrom DATETIME = NULL,
                @CreatedTo DATETIME = NULL,
                @UpdatedFrom DATETIME = NULL,
                @UpdatedTo DATETIME = NULL,
                @SortBy NVARCHAR(50) = 'Name',
                @SortOrder NVARCHAR(10) = 'ASC',
                @Limit INT = 20,
                @Offset INT = 0
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @UserAfterState NVARCHAR(MAX);
                DECLARE @UserBeforeState NVARCHAR(MAX);
                DECLARE @NOTIF_UNREAD INT = 1;
                DECLARE @NOTIF_SYSTEM INT = 3;
                -------------------------------------------------------------
                -- CREATE USER
                -------------------------------------------------------------
                IF @Action = 'CREATE'
                BEGIN
                    SET NOCOUNT ON;

                    -------------------------------------------------------
                    -- VALIDATIONS
                    -------------------------------------------------------
                    IF EXISTS (SELECT 1 FROM t_User WHERE Email = @Email)
                        THROW 50010, 'EMAIL_ALREADY_EXISTS', 1;

                    IF NOT EXISTS (SELECT 1 FROM t_Branch WHERE BranchId = @BranchId)
                        THROW 50003, 'BRANCH_NOT_FOUND', 1;

                    -------------------------------------------------------
                    -- CREATE USER
                    -------------------------------------------------------
                    INSERT INTO t_User
                    (
                        Name,
                        Email,
                        Role,
                        BranchId,
                        PasswordHash,
                        Status,
                        FalseAttempt,
                        IsLocked,
                        CreatedAt
                    )
                    VALUES
                    (
                        @Name,
                        @Email,
                        @Role,
                        @BranchId,
                        @PasswordHash,
                        1,
                        0,
                        0,
                        GETUTCDATE()
                    );

                    DECLARE @NewUserId INT = SCOPE_IDENTITY();

                    -------------------------------------------------------
                    -- NOTIFICATION LOGIC
                    -------------------------------------------------------

                   

                    -------------------------------------------------------
                    -- 1. WELCOME NOTIFICATION TO NEW USER
                    -------------------------------------------------------
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
                        @NewUserId,
                        CONCAT('Welcome ', @Name, '! Your account has been successfully created.'),
                        @NOTIF_UNREAD,
                        @NOTIF_SYSTEM,
                        GETUTCDATE()
                    );

                    -------------------------------------------------------
                    -- 2. IF NEW USER IS OFFICER → NOTIFY MANAGER
                    -------------------------------------------------------
                    IF @Role = 1
                    BEGIN
                        DECLARE @ManagerId INT;

                        SELECT TOP 1 @ManagerId = UserId
                        FROM t_User
                        WHERE BranchId = @BranchId
                            AND Role = 2;

                        IF @ManagerId IS NOT NULL
                        BEGIN
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
                                @ManagerId,
                                CONCAT('New officer ', @Name, ' has joined your branch.'),
                                @NOTIF_UNREAD,
                                @NOTIF_SYSTEM,
                                GETUTCDATE()
                            );
                        END
                    END

                    -------------------------------------------------------
                    -- 3. IF NEW USER IS MANAGER → NOTIFY ALL BRANCH USERS
                    -------------------------------------------------------
                    IF @Role = 2
                    BEGIN
                        INSERT INTO t_Notification
                        (
                            UserId,
                            Message,
                            Status,
                            Type,
                            CreatedDate
                        )
                        SELECT
                            UserId,
                            CONCAT('New branch manager ', @Name, ' has been assigned to your branch.'),
                            @NOTIF_UNREAD,
                            @NOTIF_SYSTEM,
                            GETUTCDATE()
                        FROM t_User
                        WHERE BranchId = @BranchId
                            AND UserId <> @NewUserId;
                    END

                    -------------------------------------------------------
                    -- AUDIT LOG (CREATE)
                    -------------------------------------------------------
                    
                    SELECT @UserAfterState = (
                        SELECT UserId, Name, Email, Role, BranchId, Status, IsLocked, CreatedAt, UpdatedAt
                        FROM t_User
                        WHERE UserId = @NewUserId
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
                        @UserId,
                        @LoginId,
                        'User',
                        @NewUserId,
                        'CREATE',
                        NULL,
                        @UserAfterState,
                        GETUTCDATE()
                    );

                    -------------------------------------------------------
                    -- RETURN CREATED USER
                    -------------------------------------------------------
                    SELECT 
                        UserId,
                        Name,
                        Email,
                        Role,
                        BranchId,
                        Status,
                        IsLocked,
                        CreatedAt,
                        UpdatedAt
                    FROM t_User
                    WHERE UserId = @NewUserId;

                    RETURN;
                END

                -------------------------------------------------------------
                -- UPDATE USER (Name/Role/Branch)
                -------------------------------------------------------------
                IF @Action = 'UPDATE'
                BEGIN
                    SET NOCOUNT ON;
 
                    -------------------------------------------------------
                    -- VALIDATIONS
                    -------------------------------------------------------
                    IF NOT EXISTS (SELECT 1 FROM t_User WHERE UserId = @UserId)
                        THROW 50004, 'USER_NOT_FOUND', 1;
 
                    -------------------------------------------------------
                    -- CAPTURE BEFORE STATE FOR AUDIT
                    -------------------------------------------------------
                    SELECT @UserBeforeState = (
                        SELECT UserId, Name, Email, Role, BranchId, Status, IsLocked, CreatedAt, UpdatedAt
                        FROM t_User
                        WHERE UserId = @UserId
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    );

                    -------------------------------------------------------
                    -- FETCH OLD DETAILS FOR COMPARISON
                    -------------------------------------------------------
                    DECLARE @OldRole INT;
                    DECLARE @OldBranchId INT;
                    DECLARE @OldName NVARCHAR(100);
 
                    SELECT 
                        @OldRole = Role,
                        @OldBranchId = BranchId,
                        @OldName = Name
                    FROM t_User
                    WHERE UserId = @UserId;
 
                    -------------------------------------------------------
                    -- UPDATE USER
                    -------------------------------------------------------
                    UPDATE t_User
                    SET
                        Name = COALESCE(@Name, Name),
                        Role = COALESCE(@Role, Role),
                        BranchId = COALESCE(@BranchId, BranchId),
                        UpdatedAt = GETUTCDATE()
                    WHERE UserId = @UserId;
 
                    -------------------------------------------------------
                    -- GET CURRENT VALUES AFTER UPDATE
                    -------------------------------------------------------
                    DECLARE @NewRole INT;
                    DECLARE @NewBranchId INT;
                    DECLARE @NewName NVARCHAR(100);
 
                    SELECT 
                        @NewRole = Role,
                        @NewBranchId = BranchId,
                        @NewName = Name
                    FROM t_User
                    WHERE UserId = @UserId;

                    -------------------------------------------------------
                    -- CAPTURE AFTER STATE FOR AUDIT
                    -------------------------------------------------------
                    
                    SELECT @UserAfterState = (
                        SELECT UserId, Name, Email, Role, BranchId, Status, IsLocked, CreatedAt, UpdatedAt
                        FROM t_User
                        WHERE UserId = @UserId
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    );
 

 
                    -------------------------------------------------------
                    -- SCENARIO 1: ONLY NAME UPDATED
                    -------------------------------------------------------
                    IF (@Name IS NOT NULL AND @Role IS NULL AND @BranchId IS NULL)
                    BEGIN
                        -- Notify user about name change
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
                            @UserId,
                            CONCAT('Your name has been updated from ', @OldName, ' to ', @NewName, '.'),
                            @NOTIF_UNREAD,
                            @NOTIF_SYSTEM,
                            GETUTCDATE()
                        );

                        -- If Officer: Notify Manager
                        IF @OldRole = 1
                        BEGIN
                            DECLARE @ManagerIdForName INT;
                            SELECT TOP 1 @ManagerIdForName = UserId
                            FROM t_User
                            WHERE BranchId = @OldBranchId AND Role = 2;

                            IF @ManagerIdForName IS NOT NULL
                            BEGIN
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
                                    @ManagerIdForName,
                                    CONCAT('Officer ', @OldName, ' has been renamed to ', @NewName, '.'),
                                    @NOTIF_UNREAD,
                                    @NOTIF_SYSTEM,
                                    GETUTCDATE()
                                );
                            END
                        END

                        -- If Manager: Notify all users in branch
                        IF @OldRole = 2
                        BEGIN
                            INSERT INTO t_Notification
                            (
                                UserId,
                                Message,
                                Status,
                                Type,
                                CreatedDate
                            )
                            SELECT
                                UserId,
                                CONCAT('Your branch manager ', @OldName, ' has been renamed to ', @NewName, '.'),
                                @NOTIF_UNREAD,
                                @NOTIF_SYSTEM,
                                GETUTCDATE()
                            FROM t_User
                            WHERE BranchId = @OldBranchId AND UserId <> @UserId;
                        END
                    END

                    -------------------------------------------------------
                    -- SCENARIO 2: ROLE UPDATED (PROMOTION/DEMOTION)
                    -------------------------------------------------------
                    IF (@Role IS NOT NULL AND @Role <> @OldRole)
                    BEGIN
                        -- Notify user about role change
                        DECLARE @OldRoleName NVARCHAR(50) = CASE @OldRole WHEN 1 THEN 'Officer' WHEN 2 THEN 'Manager' WHEN 3 THEN 'Admin' END;
                        DECLARE @NewRoleName NVARCHAR(50) = CASE @NewRole WHEN 1 THEN 'Officer' WHEN 2 THEN 'Manager' WHEN 3 THEN 'Admin' END;

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
                            @UserId,
                            CONCAT('Your role has been updated from ', @OldRoleName, ' to ', @NewRoleName, '.'),
                            @NOTIF_UNREAD,
                            @NOTIF_SYSTEM,
                            GETUTCDATE()
                        );

                        -- If Officer promoted to Manager: Notify all users in that branch
                        IF @OldRole = 1 AND @NewRole = 2
                        BEGIN
                            INSERT INTO t_Notification
                            (
                                UserId,
                                Message,
                                Status,
                                Type,
                                CreatedDate
                            )
                            SELECT
                                UserId,
                                CONCAT(@NewName, ' has been promoted to Branch Manager.'),
                                @NOTIF_UNREAD,
                                @NOTIF_SYSTEM,
                                GETUTCDATE()
                            FROM t_User
                            WHERE BranchId = @OldBranchId AND UserId <> @UserId;
                        END

                        -- If Manager demoted to Officer: Notify all users in that branch
                        IF @OldRole = 2 AND @NewRole = 1
                        BEGIN
                            INSERT INTO t_Notification
                            (
                                UserId,
                                Message,
                                Status,
                                Type,
                                CreatedDate
                            )
                            SELECT
                                UserId,
                                CONCAT(@NewName, ' has been demoted from Branch Manager to Officer.'),
                                @NOTIF_UNREAD,
                                @NOTIF_SYSTEM,
                                GETUTCDATE()
                            FROM t_User
                            WHERE BranchId = @OldBranchId AND UserId <> @UserId;
                        END
                    END

                    -------------------------------------------------------
                    -- SCENARIO 3: BRANCH UPDATED (TRANSFER)
                    -------------------------------------------------------
                    IF (@BranchId IS NOT NULL AND @BranchId <> @OldBranchId)
                    BEGIN
                        DECLARE @OldBranchManagerId INT;
                        DECLARE @NewBranchManagerId INT;
                        DECLARE @OldBranchName NVARCHAR(100);
                        DECLARE @NewBranchName NVARCHAR(100);

                        -- Get old and new branch names
                        SELECT @OldBranchName = BranchName FROM t_Branch WHERE BranchId = @OldBranchId;
                        SELECT @NewBranchName = BranchName FROM t_Branch WHERE BranchId = @NewBranchId;

                        -- Notify user about transfer
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
                            @UserId,
                            CONCAT('You have been transferred from ', @OldBranchName, ' to ', @NewBranchName, '.'),
                            @NOTIF_UNREAD,
                            @NOTIF_SYSTEM,
                            GETUTCDATE()
                        );

                        -- IF OFFICER TRANSFERRED
                        IF @NewRole = 1
                        BEGIN
                            -- Notify old branch manager about officer transfer
                            SELECT TOP 1 @OldBranchManagerId = UserId
                            FROM t_User
                            WHERE BranchId = @OldBranchId AND Role = 2;

                            IF @OldBranchManagerId IS NOT NULL
                            BEGIN
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
                                    @OldBranchManagerId,
                                    CONCAT('Officer ', @NewName, ' has been transferred to ', @NewBranchName, ' branch.'),
                                    @NOTIF_UNREAD,
                                    @NOTIF_SYSTEM,
                                    GETUTCDATE()
                                );
                            END

                            -- Notify new branch manager about new officer arrival
                            SELECT TOP 1 @NewBranchManagerId = UserId
                            FROM t_User
                            WHERE BranchId = @NewBranchId AND Role = 2;

                            IF @NewBranchManagerId IS NOT NULL
                            BEGIN
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
                                    @NewBranchManagerId,
                                    CONCAT('New officer ', @NewName, ' has joined your branch.'),
                                    @NOTIF_UNREAD,
                                    @NOTIF_SYSTEM,
                                    GETUTCDATE()
                                );
                            END
                        END

                        -- IF MANAGER TRANSFERRED (RELIEVED FROM OLD BRANCH)
                        IF @NewRole = 2
                        BEGIN
                            -- Notify all users in old branch that manager is relieved
                            INSERT INTO t_Notification
                            (
                                UserId,
                                Message,
                                Status,
                                Type,
                                CreatedDate
                            )
                            SELECT
                                UserId,
                                CONCAT('Your Branch Manager ', @NewName, ' has been relieved from ', @OldBranchName, ' branch.'),
                                @NOTIF_UNREAD,
                                @NOTIF_SYSTEM,
                                GETUTCDATE()
                            FROM t_User
                            WHERE BranchId = @OldBranchId AND UserId <> @UserId;

                            -- Notify all users in new branch about new manager
                            INSERT INTO t_Notification
                            (
                                UserId,
                                Message,
                                Status,
                                Type,
                                CreatedDate
                            )
                            SELECT
                                UserId,
                                CONCAT('New Branch Manager ', @NewName, ' has been assigned to ', @NewBranchName, ' branch.'),
                                @NOTIF_UNREAD,
                                @NOTIF_SYSTEM,
                                GETUTCDATE()
                            FROM t_User
                            WHERE BranchId = @NewBranchId AND UserId <> @UserId;
                        END
                    END

                    -------------------------------------------------------
                    -- SCENARIO 4: BOTH ROLE AND BRANCH UPDATED
                    -------------------------------------------------------
                    IF (@Role IS NOT NULL AND @Role <> @OldRole AND @BranchId IS NOT NULL AND @BranchId <> @OldBranchId)
                    BEGIN
                        DECLARE @OldBranchMgrId INT;
                        DECLARE @NewBranchMgrId INT;

                        -- If Officer with role change and branch change
                        IF @OldRole = 1 AND @NewRole = 2
                        BEGIN
                            -- Notify old branch manager
                            SELECT TOP 1 @OldBranchMgrId = UserId
                            FROM t_User
                            WHERE BranchId = @OldBranchId AND Role = 2;

                            IF @OldBranchMgrId IS NOT NULL
                            BEGIN
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
                                    @OldBranchMgrId,
                                    CONCAT('Officer ', @NewName, ' has been promoted to Manager and transferred.'),
                                    @NOTIF_UNREAD,
                                    @NOTIF_SYSTEM,
                                    GETUTCDATE()
                                );
                            END

                            -- Notify new branch users about new manager
                            INSERT INTO t_Notification
                            (
                                UserId,
                                Message,
                                Status,
                                Type,
                                CreatedDate
                            )
                            SELECT
                                UserId,
                                CONCAT('New Branch Manager ', @NewName, ' has been assigned to your branch.'),
                                @NOTIF_UNREAD,
                                @NOTIF_SYSTEM,
                                GETUTCDATE()
                            FROM t_User
                            WHERE BranchId = @NewBranchId AND UserId <> @UserId;
                        END
                    END

                    -------------------------------------------------------
                    -- AUDIT LOG (UPDATE)
                    -------------------------------------------------------
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
                        @PerformedBy,
                        @LoginId,
                        'User',
                        @UserId,
                        'UPDATE',
                        @UserBeforeState,
                        @UserAfterState,
                        GETUTCDATE()
                    );
 
                    -------------------------------------------------------
                    -- RETURN UPDATED USER
                    -------------------------------------------------------
                    SELECT 
                        UserId,
                        Name,
                        Email,
                        Role,
                        BranchId,
                        Status,
                        IsLocked,
                        CreatedAt,
                        UpdatedAt
                    FROM t_User
                    WHERE UserId = @UserId;
                    RETURN;
                END

                -------------------------------------------------------------
                -- UPDATE STATUS / LOCK
                -------------------------------------------------------------
                IF @Action = 'UPDATE_STATUS'
                BEGIN
                    SET NOCOUNT ON;

                    IF NOT EXISTS (SELECT 1 FROM t_User WHERE UserId = @UserId)
                        THROW 50004, 'USER_NOT_FOUND', 1;

                    -------------------------------------------------------
                    -- CAPTURE BEFORE STATE FOR AUDIT
                    -------------------------------------------------------
                    
                    SELECT @UserBeforeState = (
                        SELECT UserId, Name, Email, Role, BranchId, Status, IsLocked, CreatedAt, UpdatedAt
                        FROM t_User
                        WHERE UserId = @UserId
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    );
            
                    UPDATE t_User
                    SET
                        Status = COALESCE(@Status, Status),
                        IsLocked = COALESCE(@IsLocked, IsLocked),
                        UpdatedAt = GETUTCDATE()
                    WHERE UserId = @UserId;

                    -------------------------------------------------------
                    -- CAPTURE AFTER STATE FOR AUDIT
                    -------------------------------------------------------
                    
                    SELECT @UserAfterState = (
                        SELECT UserId, Name, Email, Role, BranchId, Status, IsLocked, CreatedAt, UpdatedAt
                        FROM t_User
                        WHERE UserId = @UserId
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    );

                    -------------------------------------------------------
                    -- AUDIT LOG (STATUS/LOCK CHANGE)
                    -------------------------------------------------------
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
                        @PerformedBy,
                        @LoginId,
                        'User',
                        @UserId,
                        'UPDATE_STATUS',
                        @UserBeforeState,
                        @UserAfterState,
                        GETUTCDATE()
                    );

                    SELECT 
                        UserId,
                        Name,
                        Email,
                        Role,
                        BranchId,
                        Status,
                        IsLocked,
                        CreatedAt,
                        UpdatedAt
                    FROM t_User
                    WHERE UserId = @UserId;
                    RETURN;
                END

                -------------------------------------------------------------
                -- CHANGE PASSWORD
                -------------------------------------------------------------
                IF @Action = 'CHANGE_PASSWORD'
                BEGIN
                    SET NOCOUNT ON;
                    -------------------------------------------------------
                    -- CAPTURE BEFORE STATE FOR AUDIT
                    -------------------------------------------------------
                   
                    SELECT @UserBeforeState = (
                        SELECT UserId, Name, Email, Role, BranchId, Status, IsLocked, CreatedAt, UpdatedAt
                        FROM t_User
                        WHERE UserId = @UserId
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                    );

                    UPDATE t_User
                    SET
                        PasswordHash = @PasswordHash,
                        UpdatedAt = GETUTCDATE()
                    WHERE UserId = @UserId;

                    IF @@ROWCOUNT = 0
                        THROW 50002, 'USER_NOT_FOUND', 1;

                    -------------------------------------------------------
                    -- AUDIT: DO NOT STORE PASSWORD HASH
                    -------------------------------------------------------
                    DECLARE @AfterInfo NVARCHAR(MAX) = (
                        SELECT UserId, Name, Email, Role, BranchId, Status, IsLocked, CreatedAt, UpdatedAt
                        FROM t_User
                        WHERE UserId = @UserId
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
                        @UserId,
                        @LoginId,
                        'User',
                        @UserId,
                        'CHANGE_PASSWORD',
                        @UserBeforeState,
                        @AfterInfo,
                        GETUTCDATE()
                    );
                    RETURN;
                END

                -------------------------------------------------------------
                -- GET USER BY ID
                -------------------------------------------------------------
                IF @Action = 'GET_BY_ID'
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        UserId,
                        Name,
                        Email,
                        Role,
                        BranchId,
                        Status,
                        IsLocked,
                        CreatedAt,
                        UpdatedAt
                    FROM t_User
                    WHERE UserId = @UserId;
                    RETURN;
                END

                -------------------------------------------------------------
                -- GET PASSWORD HASH
                -------------------------------------------------------------
                IF @Action = 'GET_PASSWORD_HASH'
                BEGIN
                    SET NOCOUNT ON;

                    SELECT PasswordHash
                    FROM t_User
                    WHERE UserId = @UserId;
                    RETURN;
                END

                -------------------------------------------------------------
                -- GET USER LIST (Basic Pagination)
                -------------------------------------------------------------
                IF @Action = 'GET_LIST'
                BEGIN
                   SET NOCOUNT ON;

                    SELECT
                        UserId,
                        Name,
                        Email,
                        Role,
                        BranchId,
                        Status,
                        IsLocked,
                        CreatedAt,
                        UpdatedAt,
                        COUNT(*) OVER() AS TotalCount
                    FROM t_User
                    WHERE
                        (@BranchId IS NULL OR BranchId = @BranchId)
                        AND (@Role IS NULL OR Role = @Role)
                        AND (@Status IS NULL OR Status = @Status)
                        AND (@IsLocked IS NULL OR IsLocked = @IsLocked)
                        AND (@Search IS NULL OR 
                                Name LIKE '%' + @Search + '%' OR 
                                Email LIKE '%' + @Search + '%')
                        AND (@CreatedFrom IS NULL OR CreatedAt >= @CreatedFrom)
                        AND (@CreatedTo IS NULL OR CreatedAt <= @CreatedTo)
                        AND (@UpdatedFrom IS NULL OR UpdatedAt >= @UpdatedFrom)
                        AND (@UpdatedTo IS NULL OR UpdatedAt <= @UpdatedTo)

                    ORDER BY
                        -- Name sorting
                        CASE WHEN @SortBy = 'Name' AND @SortOrder = 'ASC' THEN Name END ASC,
                        CASE WHEN @SortBy = 'Name' AND @SortOrder = 'DESC' THEN Name END DESC,

                        -- Email sorting
                        CASE WHEN @SortBy = 'Email' AND @SortOrder = 'ASC' THEN Email END ASC,
                        CASE WHEN @SortBy = 'Email' AND @SortOrder = 'DESC' THEN Email END DESC,

                        -- CreatedAt sorting
                        CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'ASC' THEN CreatedAt END ASC,
                        CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'DESC' THEN CreatedAt END DESC,

                        -- UpdatedAt sorting
                        CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'ASC' THEN UpdatedAt END ASC,
                        CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'DESC' THEN UpdatedAt END DESC,

                        -- Role sorting
                        CASE WHEN @SortBy = 'Role' AND @SortOrder = 'ASC' THEN Role END ASC,
                        CASE WHEN @SortBy = 'Role' AND @SortOrder = 'DESC' THEN Role END DESC,

                        -- Status sorting
                        CASE WHEN @SortBy = 'Status' AND @SortOrder = 'ASC' THEN Status END ASC,
                        CASE WHEN @SortBy = 'Status' AND @SortOrder = 'DESC' THEN Status END DESC,

                        -- IsLocked sorting
                        CASE WHEN @SortBy = 'IsLocked' AND @SortOrder = 'ASC' THEN IsLocked END ASC,
                        CASE WHEN @SortBy = 'IsLocked' AND @SortOrder = 'DESC' THEN IsLocked END DESC

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
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_User]");
        }
    }
}