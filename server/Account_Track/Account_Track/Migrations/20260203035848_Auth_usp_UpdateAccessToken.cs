using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Auth_usp_UpdateAccessToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
             CREATE PROCEDURE [dbo].[usp_UpdateAccessToken] 
                @UserId INT,
                @AccessToken VARCHAR(255)
            AS
            BEGIN
                UPDATE t_User
                SET
                    AccessToken = @AccessToken,
                    UpdatedAt = GETUTCDATE(),
                    FalseAttempt = 0
                WHERE UserId = @UserId
            END
            ";
           
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF OBJECT_ID('[dbo].[usp_UpdateAccessToken]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_UpdateAccessToken];");
        }
    }
}
