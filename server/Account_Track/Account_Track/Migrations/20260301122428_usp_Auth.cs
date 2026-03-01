using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class usp_Auth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_Auth]
            (
                @Action VARCHAR(40),

                @UserId INT = NULL,
                @Email VARCHAR(100) = NULL,
                @RefreshToken VARCHAR(500) = NULL,
                @RefreshTokenExpiry DATETIME = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                ---------------------------------------------------------
                -- GET USER BY EMAIL
                ---------------------------------------------------------
                IF @Action = 'GET_USER_BY_EMAIL'
                BEGIN
                    SELECT UserId, Role, Email, PasswordHash, IsLocked, Status, UpdatedAt
                    FROM t_User
                    WHERE Email = @Email
                    RETURN;
                END

                ---------------------------------------------------------
                -- GET USER BY ID
                ---------------------------------------------------------
                IF @Action = 'GET_USER_BY_ID'
                BEGIN
                    SELECT*
                    FROM t_User
                    WHERE UserId = @UserId
                    RETURN;
                END

                ---------------------------------------------------------
                -- INSERT LOGIN LOG
                ---------------------------------------------------------
                IF @Action = 'INSERT_LOGIN'
                BEGIN
                    SET NOCOUNT ON;

                    INSERT INTO t_LoginLog
                    (
                        UserId,
                        LoginAt,
                        RefreshToken,
                        RefreshTokenExpiry,
                        IsRevoked
                    )
                    VALUES
                    (
                        @UserId,
                        GETUTCDATE(),
                        @RefreshToken,
                        @RefreshTokenExpiry,
                        0
                    );

                    -- Return the newly created LoginId safely
                    SELECT CAST(SCOPE_IDENTITY() AS INT) AS LoginId;
                    RETURN;
                END

                ---------------------------------------------------------
                -- GET VALID LOGIN LOG
                ---------------------------------------------------------
                IF @Action = 'GET_VALID_LOGIN'
                BEGIN
                    SELECT TOP 1 *
                    FROM t_LoginLog
                    WHERE UserId = @UserId
                      AND RefreshToken = @RefreshToken
                      AND IsRevoked = 0
                      AND RefreshTokenExpiry > GETUTCDATE()
                    ORDER BY LoginAt DESC;
                    RETURN;
                END

                ---------------------------------------------------------
                -- REVOKE SPECIFIC TOKEN
                ---------------------------------------------------------
                IF @Action = 'REVOKE_TOKEN'
                BEGIN
                    UPDATE t_LoginLog
                    SET IsRevoked = 1
                    WHERE UserId = @UserId
                        AND RefreshToken = @RefreshToken;
                    RETURN;
                END

                ---------------------------------------------------------
                -- REVOKE EXPIRED TOKEN
                ---------------------------------------------------------
                IF @Action = 'REVOKE_EXPIRED'
                BEGIN
                    UPDATE t_LoginLog
                    SET IsRevoked = 1
                    WHERE UserId = @UserId
                      AND RefreshToken = @RefreshToken
                      AND RefreshTokenExpiry <= GETUTCDATE();
                    RETURN;
                END

                ---------------------------------------------------------
                -- LOGOUT ALL SESSIONS
                ---------------------------------------------------------
                IF @Action = 'LOGOUT_ALL'
                BEGIN
                    UPDATE t_LoginLog
                    SET IsRevoked = 1
                    WHERE UserId = @UserId
                        AND IsRevoked = 0;
                    RETURN;
                END

                ---------------------------------------------------------
                -- RESET FAILED ATTEMPTS
                ---------------------------------------------------------
                IF @Action = 'RESET_ATTEMPTS'
                BEGIN
                    UPDATE t_User
                    SET FalseAttempt = 0
                    WHERE UserId = @UserId;
                    RETURN;
                END

                ---------------------------------------------------------
                -- FAILED ATTEMPT (LOCK AFTER 3)
                ---------------------------------------------------------
                IF @Action = 'FAILED_ATTEMPT'
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
                    RETURN;
                END
            END
            ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_Auth]");
        }
    }
}
