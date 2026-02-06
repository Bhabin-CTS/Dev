using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Account_usp_Account_Create : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[usp_Account_Create]
    @PerformedByUserId INT,
    @CustomerName NVARCHAR(100),
    @AccountType INT,
    @InitialDeposit DECIMAL(18,2),
    @Remarks NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF (@CustomerName IS NULL OR LTRIM(RTRIM(@CustomerName)) = '')
        BEGIN
            SELECT 0 AS Success, 'INVALID_REQUEST' AS ErrorCode, 'customerName is required' AS Message,
                   NULL AS AccountId, NULL AS AccountNumber, NULL AS AccountType, NULL AS Status,
                   NULL AS Balance, NULL AS CreatedAt;
            RETURN;
        END

        IF (@InitialDeposit IS NULL OR @InitialDeposit < 0)
        BEGIN
            SELECT 0 AS Success, 'INVALID_AMOUNT' AS ErrorCode, 'initialDeposit must be >= 0' AS Message,
                   NULL AS AccountId, NULL AS AccountNumber, NULL AS AccountType, NULL AS Status,
                   NULL AS Balance, NULL AS CreatedAt;
            RETURN;
        END

        DECLARE @BranchId INT, @Now DATETIME2 = SYSUTCDATETIME();
        SELECT @BranchId = u.BranchId
        FROM dbo.t_User u WITH (NOLOCK)
        WHERE u.UserId = @PerformedByUserId;

        IF (@BranchId IS NULL)
        BEGIN
            SELECT 0 AS Success, 'USER_NOT_FOUND' AS ErrorCode, 'PerformedBy user invalid' AS Message,
                   NULL AS AccountId, NULL AS AccountNumber, NULL AS AccountType, NULL AS Status,
                   NULL AS Balance, NULL AS CreatedAt;
            RETURN;
        END

        -- Generate unique AccountNumber with serialization to avoid duplicates
        DECLARE @AccountNumber INT;
        ;WITH mx AS (
            SELECT MAX(a.AccountNumber) AS MaxAcc
            FROM dbo.t_Account a WITH (UPDLOCK, HOLDLOCK)
        )
        SELECT @AccountNumber = ISNULL(MaxAcc, 1000000) + 1 FROM mx;

        DECLARE @AccountId INT;
        INSERT INTO dbo.t_Account
        (
            CustomerName, AccountNumber, BranchId, AccountType, Balance, Status,
            CreatedByUserId, CreatedAt, UpdatedAt
        )
        VALUES
        (
            @CustomerName, @AccountNumber, @BranchId, @AccountType,
            @InitialDeposit, 1,   -- Status=Active (1)
            @PerformedByUserId, @Now, NULL
        );

        SET @AccountId = SCOPE_IDENTITY();

        SELECT
            1 AS Success,
            NULL AS ErrorCode,
            'Account created successfully' AS Message,
            @AccountId AS AccountId,
            @AccountNumber AS AccountNumber,
            @AccountType AS AccountType,
            1 AS Status,
            CAST(@InitialDeposit AS DECIMAL(18,2)) AS Balance,
            @Now AS CreatedAt;
    END TRY
    BEGIN CATCH
        SELECT 0 AS Success,
               'DB_ERROR' AS ErrorCode,
               ERROR_MESSAGE() AS Message,
               NULL AS AccountId, NULL AS AccountNumber, NULL AS AccountType, NULL AS Status,
               NULL AS Balance, NULL AS CreatedAt;
    END CATCH
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('[dbo].[usp_Account_Create]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[usp_Account_Create];
");
        }
    }
}
