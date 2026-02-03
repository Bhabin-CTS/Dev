using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Transaction_usp_GetTransactionDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE PROCEDURE [dbo].[usp_GetTransactionById]
                @TransactionId INT,
                @UserId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @UserBranchId INT;
                DECLARE @UserRole INT;

                DECLARE @ROLE_ADMIN INT = 3;

                -- Get user info from DB
                SELECT 
                    @UserBranchId = BranchId,
                    @UserRole = Role
                FROM t_User
                WHERE UserId = @UserId;

                SELECT
                    TransactionId,
                    CreatedByUserId As CreatedBy,
                    Type,
                    Amount,
                    Status,
                    IsHighValue,
                    FromAccountId,
                    ToAccountId,
                    BalanceBefore,
                    BalanceAfterTxn as BalanceAfter,
                    CreatedAt,
                    UpdatedAt
                FROM t_Transaction
                WHERE TransactionId = @TransactionId
                AND (
                        @UserRole = @ROLE_ADMIN
                        OR BranchId = @UserBranchId
                    );
            END
            ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"IF OBJECT_ID('[dbo].[usp_GetTransactionById]', 'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[usp_GetTransactionById];");
        }
    }
}
