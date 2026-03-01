using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class user_usp_CreateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
        CREATE OR ALTER PROCEDURE [dbo].[usp_CreateUser]
            @Name NVARCHAR(100),
            @Email NVARCHAR(150),
            @Role INT,
            @BranchId INT,
            @PasswordHash NVARCHAR(200),
            @UserId INT,
            @LoginId INT
        AS
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

            DECLARE @NOTIF_UNREAD INT = 1;
            DECLARE @NOTIF_SYSTEM INT = 3;

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
            DECLARE @UserAfterState NVARCHAR(MAX);
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

        END
        ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_CreateUser]");
        }
    }
}
