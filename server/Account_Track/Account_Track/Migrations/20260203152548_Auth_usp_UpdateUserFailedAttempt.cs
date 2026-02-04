using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Auth_usp_UpdateUserFailedAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE PROCEDURE [dbo].[usp_UpdateUserFailedAttempt]
                @UserId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                UPDATE t_User
                SET 
                    FalseAttempt = FalseAttempt + 1,
                    IsLocked = CASE 
                                 WHEN FalseAttempt + 1 >= 3 THEN 1 
                                 ELSE IsLocked 
                               END
                WHERE UserId = @UserId;
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
               @"IF OBJECT_ID('[dbo].[usp_UpdateUserFailedAttempt]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_UpdateUserFailedAttempt];");
        }
    }
}
