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
            CREATE OR ALTER PROCEDURE [dbo].[usp_InsertLoginLog]
                @UserId INT,
                @RefreshToken VARCHAR(255),
                @RefreshTokenExpiry DATETIME
            AS
            BEGIN
                SET NOCOUNT ON;
                INSERT INTO t_LoginLog (UserId, LoginAt, RefreshToken, RefreshTokenExpiry, IsRevoked)
                VALUES (@UserId, GETUTCDATE(), @RefreshToken, @RefreshTokenExpiry, 0);
                DECLARE @NewLoginId INT = SCOPE_IDENTITY();

                SELECT @NewLoginId AS  LoginId;
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
