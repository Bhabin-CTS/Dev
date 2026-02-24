using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_Notification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
                CREATE OR ALTER PROCEDURE [dbo].[usp_Notification]
                (
                    @Action VARCHAR(20),

                    @UserId INT,

                    @NotificationIds NVARCHAR(MAX) = NULL
                )
                AS
                BEGIN
                    SET NOCOUNT ON;

                    ----------------------------------------------------------------
                    -- 1️⃣ GET NOTIFICATIONS (Unread)
                    ----------------------------------------------------------------
                    IF @Action = 'GET'
                    BEGIN
                        SELECT NotificationId, Message, Type, CreatedDate
                        FROM t_Notification
                        WHERE UserId = @UserId
                            AND Status = 1 -- Only unread notifications
                        ORDER BY CreatedDate DESC;

                        RETURN;
                    END

                    ----------------------------------------------------------------
                    -- 2️⃣ UPDATE NOTIFICATIONS (Mark As Read)
                    ----------------------------------------------------------------
                    IF @Action = 'UPDATE'
                    BEGIN
                        UPDATE t_Notification
                        SET Status = 2, UpdatedAt = GETUTCDATE()
                        WHERE UserId = @UserId
                          AND NotificationId IN (SELECT value FROM STRING_SPLIT(@NotificationIds, ','));
                        RETURN;
                    END
                END;
                ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
