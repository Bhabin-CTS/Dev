using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Auth_usp_InsertLoginLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE PROCEDURE [dbo].[usp_InsertLoginLog]
                @UserId INT,
                @RefreshToken VARCHAR(255),
                @RefreshTokenExpiry DATETIME
            AS
            BEGIN
                INSERT INTO t_LoginLog (UserId, LoginAt, RefreshToken, RefreshTokenExpiry, IsRevoked)
                VALUES (@UserId, GETUTCDATE(), @RefreshToken, @RefreshTokenExpiry, 0);
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               @"IF OBJECT_ID('[dbo].[usp_InsertLoginLog]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_InsertLoginLog];");
        }
    }
}
