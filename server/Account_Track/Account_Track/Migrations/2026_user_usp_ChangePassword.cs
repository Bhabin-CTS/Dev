using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class user_usp_ChangePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_ChangePassword]
                @UserId INT,
                @PasswordHash NVARCHAR(200),
                @LoginId INT
            AS
            BEGIN
                SET NOCOUNT ON;
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
            END";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_ChangePassword]");
        }
    }
}
