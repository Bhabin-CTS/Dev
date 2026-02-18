using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Account_usp_GetAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.usp_GetAccounts
    @AccountNumber INT = NULL,
    @AccountType INT = NULL,
    @Status INT = NULL,
    @Search NVARCHAR(100) = NULL,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @SortBy NVARCHAR(50) = NULL,      -- createdAt|customerName|accountNumber|balance
    @SortOrder NVARCHAR(4) = 'ASC',   -- ASC|DESC
    @Limit INT = 20,
    @Offset INT = 0,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserBranchId INT, @UserRole INT, @IsAdmin BIT = 0;
    SELECT @UserBranchId = u.BranchId, @UserRole = u.Role
    FROM dbo.t_User u WITH (NOLOCK)
    WHERE u.UserId = @UserId;

    IF (@UserRole = 3) SET @IsAdmin = 1; -- Adjust if Admin != 3

    -- Whitelist sort column
    IF (@SortBy NOT IN ('createdAt','customerName','accountNumber','balance'))
        SET @SortBy = 'createdAt';
    IF (@SortOrder NOT IN ('ASC','DESC'))
        SET @SortOrder = 'ASC';

    DECLARE @sql NVARCHAR(MAX) = N'
    WITH base AS (
        SELECT
            a.AccountId,
            a.AccountNumber,
            a.CustomerName,
            a.AccountType,
            a.Status,
            a.Balance,
            a.CreatedAt
        FROM dbo.t_Account a WITH (NOLOCK)
        WHERE ( @AccountNumber IS NULL OR a.AccountNumber = @AccountNumber )
          AND ( @AccountType  IS NULL OR a.AccountType  = @AccountType )
          AND ( @Status       IS NULL OR a.Status       = @Status )
          AND ( @Search IS NULL OR a.CustomerName LIKE ''%'' + @Search + ''%'' )
          AND ( @FromDate IS NULL OR a.CreatedAt >= @FromDate )
          AND ( @ToDate   IS NULL OR a.CreatedAt < DATEADD(DAY, 1, @ToDate) )
          AND ( @IsAdmin = 1 OR a.BranchId = @UserBranchId )
    ),
    cnt AS ( SELECT COUNT(1) AS TotalCount FROM base )
    SELECT
        b.AccountId, b.AccountNumber, b.CustomerName,
        b.AccountType, b.Status, b.Balance, b.CreatedAt,
        c.TotalCount
    FROM base b
    CROSS JOIN cnt c
    ORDER BY ';

    -- dynamic ORDER BY (safe because of whitelist)
    SET @sql += CASE @SortBy
        WHEN 'createdAt'     THEN ' b.CreatedAt '
        WHEN 'customerName'  THEN ' b.CustomerName '
        WHEN 'accountNumber' THEN ' b.AccountNumber '
        WHEN 'balance'       THEN ' b.Balance '
        ELSE ' b.CreatedAt '
    END + ' ' + @SortOrder + '
    OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;';

    EXEC sp_executesql @sql,
        N'@AccountNumber INT, @AccountType INT, @Status INT, @Search NVARCHAR(100),
          @FromDate DATETIME2, @ToDate DATETIME2, @Limit INT, @Offset INT,
          @UserId INT, @UserBranchId INT, @IsAdmin BIT',
        @AccountNumber = @AccountNumber,
        @AccountType = @AccountType,
        @Status = @Status,
        @Search = @Search,
        @FromDate = @FromDate,
        @ToDate = @ToDate,
        @Limit = @Limit,
        @Offset = @Offset,
        @UserId = @UserId,
        @UserBranchId = @UserBranchId,
        @IsAdmin = @IsAdmin;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.usp_GetAccounts', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetAccounts;
");

        }
    }
}
