using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Transaction_usp_TransactionList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE PROCEDURE [dbo].[usp_GetTransactions]
                @AccountId INT = NULL,
                @Type VARCHAR(20) = NULL,
                @Status VARCHAR(20) = NULL,
                @IsHighValue BIT = NULL,
                @FromDate DATE = NULL,
                @ToDate DATE = NULL,
                @Limit INT = 20,
                @Offset INT = 0,
                @userId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @UserBranchId INT;
                DECLARE @UserRole INT;

                DECLARE @ROLE_ADMIN INT = 3;
                DECLARE @ROLE_MANAGER INT = 2;
                DECLARE @ROLE_OFFICER INT = 1;
                -- Fetch user details from DB (TRUSTED SOURCE)
                SELECT 
                    @UserBranchId = BranchId,
                    @UserRole = Role
                FROM t_User
                WHERE UserId = @UserId;


                SELECT
                    t.TransactionId,
                    t.Type,
                    t.Amount,
                    t.Status,
                    t.IsHighValue,
                    t.CreatedAt,
                    COUNT(*) OVER() AS TotalCount
                FROM t_Transaction t
                WHERE
                    (@AccountId IS NULL OR t.FromAccountId = @AccountId OR t.ToAccountId = @AccountId)
                    AND (@Type IS NULL OR t.Type = @Type)
                    AND (@Status IS NULL OR t.Status = @Status)
                    AND (@IsHighValue IS NULL OR t.IsHighValue = @IsHighValue)
                    AND (@FromDate IS NULL OR t.CreatedAt >= @FromDate)
                    AND (@ToDate IS NULL OR t.CreatedAt <= @ToDate)

                    -- SECURITY FILTER
                    AND (
                        @UserRole = @ROLE_ADMIN
                        OR
                        t.BranchId = @UserBranchId
                    )

                ORDER BY t.CreatedAt DESC
                OFFSET @Offset ROWS
                FETCH NEXT @Limit ROWS ONLY;
            END
            ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF OBJECT_ID('[dbo].[usp_GetTransactions]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_GetTransactions];");
        }
    }
}
