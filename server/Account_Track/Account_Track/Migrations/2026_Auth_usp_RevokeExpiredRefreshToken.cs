using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Auth_usp_RevokeExpiredRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE PROCEDURE [dbo].[usp_RevokeExpiredRefreshToken]
                @UserId INT,
                @RefreshToken VARCHAR(255)
            AS
            BEGIN
                UPDATE t_LoginLog
                SET IsRevoked = 1
                WHERE UserId = @UserId
                  AND RefreshToken = @RefreshToken
                  AND RefreshTokenExpiry <= GETUTCDATE();
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               @"IF OBJECT_ID('[dbo].[usp_RevokeExpiredRefreshToken]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_RevokeExpiredRefreshToken];");
        }
    }
}
