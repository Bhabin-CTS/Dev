using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Auth_usp_GetValidLoginLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_GetValidLoginLog]
                @UserId INT,
                @RefreshToken VARCHAR(255)
            AS
            BEGIN
                SELECT TOP 1 *
                FROM t_LoginLog
                WHERE UserId = @UserId
                  AND RefreshToken = @RefreshToken
                  AND IsRevoked = 0
                  AND RefreshTokenExpiry > GETUTCDATE()
                ORDER BY LoginAt DESC;
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               @"IF OBJECT_ID('[dbo].[usp_GetValidLoginLog]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_GetValidLoginLog];");
        }
    }
}
