using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class user_usp_GetUserById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
        CREATE OR ALTER PROCEDURE [dbo].[usp_GetUserById]
            @UserId INT
        AS
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
        END";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_GetUserById]");
        }
    }
}
