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
            @PerformedBy INT
        AS
        BEGIN
            SET NOCOUNT ON;
 
            -------------------------------------------------------
            -- VALIDATIONS
            -------------------------------------------------------
            IF NOT EXISTS (SELECT 1 FROM t_User WHERE UserId = @UserId)
                THROW 50004, 'USER_NOT_FOUND', 1;
 
            -------------------------------------------------------
            -- FETCH OLD DETAILS FOR COMPARISON
            -------------------------------------------------------
            DECLARE @OldRole INT;
            DECLARE @OldBranchId INT;
            DECLARE @UserName NVARCHAR(100);
 
            SELECT 
                @OldRole = Role,
                @OldBranchId = BranchId,
                @UserName = Name
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
            -- NOTIFICATION LOGIC
            -------------------------------------------------------
 
            DECLARE @NOTIF_UNREAD INT = 1;
            DECLARE @NOTIF_SYSTEM INT = 3;
 
            -------------------------------------------------------
            -- 1. NOTIFY USER HIMSELF ABOUT UPDATE
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
                @UserId,
                'Your user details have been updated by administrator.',
                @NOTIF_UNREAD,
                @NOTIF_SYSTEM,
                GETUTCDATE()
            );
 
            -------------------------------------------------------
            -- GET CURRENT VALUES AFTER UPDATE
            -------------------------------------------------------
            DECLARE @NewRole INT;
            DECLARE @NewBranchId INT;
 
            SELECT 
                @NewRole = Role,
                @NewBranchId = BranchId
            FROM t_User
            WHERE UserId = @UserId;
 
            -------------------------------------------------------
            -- 2. IF UPDATED USER IS OFFICER → NOTIFY MANAGER
            -------------------------------------------------------
            IF @NewRole = 1   -- Officer
            BEGIN
                DECLARE @ManagerId INT;
 
                SELECT TOP 1 @ManagerId = UserId
                FROM t_User
                WHERE BranchId = @NewBranchId
                  AND Role = 2;   -- Manager
 
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
                        CONCAT('Details of officer ', @UserName, ' have been updated.'),
                        @NOTIF_UNREAD,
                        @NOTIF_SYSTEM,
                        GETUTCDATE()
                    );
                END
            END
 
            -------------------------------------------------------
            -- 3. IF UPDATED USER IS MANAGER → NOTIFY ALL USERS
            -------------------------------------------------------
            IF @NewRole = 2   -- Manager
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
                    CONCAT('Branch manager ', @UserName, ' details have been updated.'),
                    @NOTIF_UNREAD,
                    @NOTIF_SYSTEM,
                    GETUTCDATE()
                FROM t_User
                WHERE BranchId = @NewBranchId
                  AND UserId <> @UserId;
            END
 
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
