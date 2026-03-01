using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class user_usp_UpdateUserStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
        CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateUserStatus]
            @UserId INT,
            @Status INT = NULL,
            @IsLocked BIT = NULL,
            @Reason NVARCHAR(500) = NULL,
            @PerformedBy INT,
            @LoginId INT
        AS
        BEGIN
            SET NOCOUNT ON;

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
            
            UPDATE t_User
            SET
                Status = COALESCE(@Status, Status),
                IsLocked = COALESCE(@IsLocked, IsLocked),
                UpdatedAt = GETUTCDATE()
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
        END";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_UpdateUserStatus]");
        }
    }
}
