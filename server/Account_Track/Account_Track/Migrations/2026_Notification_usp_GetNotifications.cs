using Account_Track.Utils.Enum;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Notification_usp_GetNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @" 
            CREATE OR ALTER PROCEDURE [dbo].[usp_GetNotifications]
                @UserId INT
            AS
            BEGIN
                SELECT NotificationId, Message, Type, CreatedDate
                FROM t_Notification
                WHERE UserId = @UserId
                    AND Status = 1 -- Only unread notifications
                ORDER BY CreatedDate DESC;
            END;
             "; 
            migrationBuilder.Sql(sp);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS usp_GetNotifications;");
        }
    }
}
