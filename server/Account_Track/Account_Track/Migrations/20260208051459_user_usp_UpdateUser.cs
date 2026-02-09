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

            IF NOT EXISTS (SELECT 1 FROM t_User WHERE UserId = @UserId)
                THROW 50004, 'USER_NOT_FOUND', 1;

            UPDATE t_User
            SET
                Name = COALESCE(@Name, Name),
                Role = COALESCE(@Role, Role),
                BranchId = COALESCE(@BranchId, BranchId),
                UpdatedAt = GETUTCDATE()
            WHERE UserId = @UserId;

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
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_UpdateUser]");
        }
    }
}
