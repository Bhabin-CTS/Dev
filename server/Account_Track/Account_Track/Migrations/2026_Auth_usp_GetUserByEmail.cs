using Account_Track.Model;
using Account_Track.Utils.Enum;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Auth_usp_GetUserByEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE PROCEDURE [dbo].[usp_GetUserByEmail] 
                @Email VARCHAR(100)
            AS
            BEGIN
                SELECT UserId, Role, Email, PasswordHash, IsLocked, Status
                FROM t_User
                WHERE Email = @Email
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF OBJECT_ID('[dbo].[usp_GetUserByEmail]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_GetUserByEmail];");
        }
    }
}
