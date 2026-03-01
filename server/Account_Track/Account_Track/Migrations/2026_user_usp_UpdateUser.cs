using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class user_usp_UpdateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
        CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateUser]
            @UserId INT,
            @Name NVARCHAR(100) = NULL,
            @Role INT = NULL,
            @BranchId INT = NULL,
            @PerformedBy INT,
            @LoginId INT
        AS
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
            DECLARE @UserBeforeState NVARCHAR(MAX);
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
            DECLARE @UserAfterState NVARCHAR(MAX);
            SELECT @UserAfterState = (
                SELECT UserId, Name, Email, Role, BranchId, Status, IsLocked, CreatedAt, UpdatedAt
                FROM t_User
                WHERE UserId = @UserId
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
            );
 
            DECLARE @NOTIF_UNREAD INT = 1;
            DECLARE @NOTIF_SYSTEM INT = 3;
 
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
        END
        ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_UpdateUser]");
        }
    }
}
