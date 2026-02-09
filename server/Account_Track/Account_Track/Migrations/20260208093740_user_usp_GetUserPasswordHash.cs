using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class user_usp_GetUserPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_GetUserPasswordHash]
                @UserId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT PasswordHash
                FROM t_User
                WHERE UserId = @UserId;
            END";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
