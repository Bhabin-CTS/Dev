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
            @UserId INT
        AS
        BEGIN
            SET NOCOUNT ON;

            IF EXISTS (SELECT 1 FROM t_User WHERE Email = @Email)
                THROW 50010, 'EMAIL_ALREADY_EXISTS', 1;

            IF NOT EXISTS (SELECT 1 FROM t_Branch WHERE BranchId = @BranchId)
                THROW 50003, 'BRANCH_NOT_FOUND', 1;

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
            WHERE UserId = SCOPE_IDENTITY();
        END";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_CreateUser]");
        }
    }
}
