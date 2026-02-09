using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Notification_usp_UpdateNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @" 
            CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateNotifications]
                @UserId INT,
                @NotificationIds NVARCHAR(MAX)
            AS
            BEGIN
                UPDATE t_Notification
                SET Status = 2, UpdatedAt = GETUTCDATE()
                WHERE UserId = @UserId
                  AND NotificationId IN (SELECT value FROM STRING_SPLIT(@NotificationIds, ','));
            END;
             ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS usp_UpdateNotifications;");
        }
    }
}
