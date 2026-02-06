using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Account_usp_GetAccountById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.usp_GetAccountById
    @AccountId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE 
        @UserBranchId INT,
        @UserRole INT,
        @IsAdmin BIT = 0;

    SELECT 
        @UserBranchId = u.BranchId, 
        @UserRole = u.Role
    FROM dbo.t_User AS u WITH (NOLOCK)
    WHERE u.UserId = @UserId;

    -- Admin = 3 per your enums
    IF (@UserRole = 3)
        SET @IsAdmin = 1;

    SELECT TOP (1)
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
        CAST(N'' as xml).value('xs:base64Binary(sql:column(""a.RowVersion""))', 'NVARCHAR(100)') AS RowVersionBase64
    FROM dbo.t_Account AS a WITH (NOLOCK)
    INNER JOIN dbo.t_Branch AS b WITH (NOLOCK) 
        ON b.BranchId = a.BranchId
    WHERE a.AccountId = @AccountId
      AND (@IsAdmin = 1 OR a.BranchId = @UserBranchId);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.usp_GetAccountById', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetAccountById;
");
        }
    }
}
