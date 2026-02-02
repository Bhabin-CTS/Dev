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
                @Offset INT = 0
            AS
            BEGIN
                SET NOCOUNT ON;
 
                ;WITH TxnCTE AS
                (
                    SELECT
                        TransactionId,
                        Type,
                        Amount,
                        Status,
                        IsHighValue,
                        CreatedAt,
                        COUNT(*) OVER() AS TotalCount
                    FROM t_Transaction
                    WHERE
                        (@AccountId IS NULL OR FromAccountId = @AccountId OR ToAccountId = @AccountId)
                        AND (@Type IS NULL OR Type = @Type)
                        AND (@Status IS NULL OR Status = @Status)
                        AND (@IsHighValue IS NULL OR IsHighValue = @IsHighValue)
                        AND (@FromDate IS NULL OR CAST(CreatedAt AS DATE) >= @FromDate)
                        AND (@ToDate IS NULL OR CAST(CreatedAt AS DATE) <= @ToDate)
                )
                SELECT *
                FROM TxnCTE
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;
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
