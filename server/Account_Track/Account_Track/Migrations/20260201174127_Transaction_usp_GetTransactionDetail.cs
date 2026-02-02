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
                @TransactionId INT
            AS
            BEGIN
                SET NOCOUNT ON;
 
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
                WHERE TransactionId = @TransactionId;
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
