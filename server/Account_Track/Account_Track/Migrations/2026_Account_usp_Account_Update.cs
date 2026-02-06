using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Account_usp_Account_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[usp_Account_Update]
    @AccountId INT,
    @CustomerName NVARCHAR(100) = NULL,
    @Status INT = NULL,
    @Remarks NVARCHAR(500) = NULL,           -- reserved, not persisted
    @RowVersionBase64 NVARCHAR(200),
    @PerformedByUserId INT,
    @AccountType INT = NULL                  -- allow changing account type
AS
BEGIN
    SET NOCOUNT ON;

    IF (@RowVersionBase64 IS NULL OR LTRIM(RTRIM(@RowVersionBase64)) = '')
    BEGIN
        SELECT 0 AS Success, 'INVALID_REQUEST' AS ErrorCode, 'rowVersionBase64 is required' AS Message,
               NULL AS AccountId, NULL AS AccountNumber, NULL AS CustomerName, NULL AS AccountType,
               NULL AS Status, NULL AS Balance, NULL AS BranchId, NULL AS BranchName, NULL AS CreatedByUserId,
               NULL AS CreatedAt, NULL AS UpdatedAt, NULL AS RowVersionBase64;
        RETURN;
    END

    -- validate account type (your enum: 1=Savings, 2=Current)
    IF (@AccountType IS NOT NULL AND @AccountType NOT IN (1,2))
    BEGIN
        SELECT 0 AS Success, 'INVALID_REQUEST' AS ErrorCode, 'accountType must be 1 (Savings) or 2 (Current)' AS Message,
               NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL;
        RETURN;
    END

    DECLARE @ExpectedRowVersion VARBINARY(8);
    SELECT @ExpectedRowVersion =
        CAST(CAST(N'' as xml).value('xs:base64Binary(sql:variable(""@RowVersionBase64""))','varbinary(max)') AS varbinary(8));

    DECLARE @UserBranchId INT, @UserRole INT, @IsAdmin BIT = 0;
    SELECT @UserBranchId = u.BranchId, @UserRole = u.Role
    FROM dbo.t_User u WITH (NOLOCK)
    WHERE u.UserId = @PerformedByUserId;

    IF (@UserRole = 3) SET @IsAdmin = 1; -- Admin=3

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();

    -- Access check first (avoid leaking rowversion info)
    IF EXISTS (
        SELECT 1 FROM dbo.t_Account a
        WHERE a.AccountId = @AccountId
          AND (@IsAdmin = 1 OR a.BranchId = @UserBranchId)
    )
    BEGIN
        UPDATE a
           SET a.CustomerName = COALESCE(@CustomerName, a.CustomerName),
               a.Status       = COALESCE(@Status, a.Status),
               a.AccountType  = COALESCE(@AccountType, a.AccountType),
               a.UpdatedAt    = @Now
        FROM dbo.t_Account a
        WHERE a.AccountId = @AccountId
          AND a.RowVersion = @ExpectedRowVersion;

        IF (@@ROWCOUNT = 0)
        BEGIN
            SELECT 0 AS Success, 'CONFLICT' AS ErrorCode, 'Record was modified by someone else' AS Message,
                   NULL AS AccountId, NULL AS AccountNumber, NULL AS CustomerName, NULL AS AccountType,
                   NULL AS Status, NULL AS Balance, NULL AS BranchId, NULL AS BranchName, NULL AS CreatedByUserId,
                   NULL AS CreatedAt, NULL AS UpdatedAt, NULL AS RowVersionBase64;
            RETURN;
        END

        -- Return full snapshot incl. branch fields and createdBy
        SELECT TOP (1)
            1 AS Success,
            NULL AS ErrorCode,
            'Account updated successfully' AS Message,
            a.AccountId,
            a.AccountNumber,
            a.CustomerName,
            a.AccountType,
            a.Status,
            a.Balance,
            a.BranchId,
            b.BranchName,
            a.CreatedByUserId,
            a.CreatedAt,
            a.UpdatedAt,
            CAST(N'' as xml).value('xs:base64Binary(sql:column(""a.RowVersion""))','NVARCHAR(100)') AS RowVersionBase64
        FROM dbo.t_Account a WITH (NOLOCK)
        INNER JOIN dbo.t_Branch b WITH (NOLOCK) ON b.BranchId = a.BranchId
        WHERE a.AccountId = @AccountId;
    END
    ELSE
    BEGIN
        SELECT 0 AS Success, 'ACCOUNT_NOT_FOUND_OR_ACCESS_DENIED' AS ErrorCode, 'Either not found or access denied' AS Message,
               NULL AS AccountId, NULL AS AccountNumber, NULL AS CustomerName, NULL AS AccountType,
               NULL AS Status, NULL AS Balance, NULL AS BranchId, NULL AS BranchName, NULL AS CreatedByUserId,
               NULL AS CreatedAt, NULL AS UpdatedAt, NULL AS RowVersionBase64;
    END
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.usp_Account_Update', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Account_Update;
");
        }
    }
}
