using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Auth_usp_ResetUserAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_ResetUserAttempts]
                @UserId INT
            AS
            BEGIN
                UPDATE t_User
                SET FalseAttempt = 0
                WHERE UserId = @UserId;
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               @"IF OBJECT_ID('[dbo].[usp_ResetUserAttempts]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_ResetUserAttempts];");
        }
    }
}
