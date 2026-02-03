using Account_Track.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Migrations;
using static System.Runtime.InteropServices.JavaScript.JSType;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Auth_usp_GetUserById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE PROCEDURE[dbo].[usp_GetUserById] 
                @UserId INT
            AS
            BEGIN
                SELECT*
                FROM t_User
                WHERE UserId = @UserId
            END
            ";
            
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               @"IF OBJECT_ID('[dbo].[usp_GetUserById]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_GetUserById];");
        }
    }
}
