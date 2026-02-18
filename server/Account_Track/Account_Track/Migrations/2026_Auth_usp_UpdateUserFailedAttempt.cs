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
            CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateUserFailedAttempt]
                @UserId INT
            AS
            BEGIN
                SET NOCOUNT ON;
 
                DECLARE @CurrentAttempts INT;
                DECLARE @UserName NVARCHAR(100);
                DECLARE @BranchId INT;
                DECLARE @IsAlreadyLocked BIT;
 
                -- Fetch current values
                SELECT 
                    @CurrentAttempts = FalseAttempt,
                    @UserName = Name,
                    @BranchId = BranchId,
                    @IsAlreadyLocked = IsLocked
                FROM t_User
                WHERE UserId = @UserId;
 
                -- Increment attempts
                UPDATE t_User
                SET 
                    FalseAttempt = FalseAttempt + 1,
                    IsLocked = CASE 
                                 WHEN FalseAttempt + 1 >= 3 THEN 1 
                                 ELSE IsLocked 
                               END
                WHERE UserId = @UserId;
 
                ----------------------------------------------------
                -- IF THIS ATTEMPT CAUSED ACCOUNT TO BE LOCKED
                ----------------------------------------------------
                IF (@CurrentAttempts + 1 >= 3 AND @IsAlreadyLocked = 0)
                BEGIN
                    DECLARE @ManagerId INT;
 
                    -- Find manager of that branch
                    SELECT TOP 1 @ManagerId = UserId
                    FROM t_User
                    WHERE BranchId = @BranchId
                      AND Role = 2;  -- Manager
 
                    IF @ManagerId IS NOT NULL
                    BEGIN
                        INSERT INTO t_Notification
                        (
                            UserId,
                            Message,
                            Status,
                            Type,
                            CreatedDate
                        )
                        VALUES
                        (
                            @ManagerId,
                            CONCAT('Suspicious login attempts detected for user ', @UserName, '. Account has been locked.'),
                            1,   -- Unread
                            2,   -- SuspiciousActivity
                            GETUTCDATE()
                        );
                    END
                END
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
