using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class user_usp_ChangePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_ChangePassword]
                @UserId INT,
                @PasswordHash NVARCHAR(200)
            AS
            BEGIN
                SET NOCOUNT ON;

                UPDATE t_User
                SET
                    PasswordHash = @PasswordHash,
                    UpdatedAt = GETUTCDATE()
                WHERE UserId = @UserId;

                IF @@ROWCOUNT = 0
                    THROW 50002, 'USER_NOT_FOUND', 1;
            END";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_ChangePassword]");
        }
    }
}
