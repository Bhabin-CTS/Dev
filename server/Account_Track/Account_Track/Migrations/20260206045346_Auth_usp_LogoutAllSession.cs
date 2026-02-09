using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Auth_usp_LogoutAllSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE PROCEDURE [dbo].[usp_LogoutAllSession]
                @UserId INT
            AS
            BEGIN
                UPDATE t_LoginLog
                SET IsRevoked = 1
                WHERE UserId = @UserId;
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               @"IF OBJECT_ID('[dbo].[usp_LogoutAllSession]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_LogoutAllSession];");
        }
    }
}
