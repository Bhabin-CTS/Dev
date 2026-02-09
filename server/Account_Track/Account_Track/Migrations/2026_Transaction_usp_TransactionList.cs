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
           CREATE OR ALTER PROCEDURE [dbo].[usp_GetTransactions]
                @AccountId INT = NULL,
                @Type VARCHAR(20) = NULL,
                @Status VARCHAR(20) = NULL,
                @IsHighValue BIT = NULL,
                @CreatedFrom DATE = NULL,
                @CreatedTo DATE = NULL,
                @UpdatedFrom DATE = NULL,
                @UpdatedTo DATE = NULL,
                @SortBy NVARCHAR(50) = 'CreatedAt',
                @SortOrder NVARCHAR(10) = 'DESC',
                @Limit INT = 20,
                @Offset INT = 0,
                @UserId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @UserBranchId INT;
                DECLARE @UserRole INT;

                DECLARE @ROLE_ADMIN INT = 3;
                DECLARE @ROLE_MANAGER INT = 2;
                DECLARE @ROLE_OFFICER INT = 1;

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
                    t.UpdatedAt,
                    COUNT(*) OVER() AS TotalCount
                FROM t_Transaction t
                WHERE
                    (@AccountId IS NULL OR t.FromAccountId = @AccountId OR t.ToAccountId = @AccountId)
                    AND (@Type IS NULL OR t.Type = @Type)
                    AND (@Status IS NULL OR t.Status = @Status)
                    AND (@IsHighValue IS NULL OR t.IsHighValue = @IsHighValue)
                    AND (@CreatedFrom IS NULL OR t.CreatedAt >= @CreatedFrom)
                    AND (@CreatedTo IS NULL OR t.CreatedAt <= @CreatedTo)
                    AND (@UpdatedFrom IS NULL OR t.UpdatedAt >= @UpdatedFrom)
                    AND (@UpdatedTo IS NULL OR t.UpdatedAt <= @UpdatedTo)
                    AND (
                        @UserRole = @ROLE_ADMIN
                        OR t.BranchId = @UserBranchId
                    )
                ORDER BY
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'ASC' THEN t.CreatedAt END ASC,
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'DESC' THEN t.CreatedAt END DESC,

                    CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'ASC' THEN t.UpdatedAt END ASC,
                    CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'DESC' THEN t.UpdatedAt END DESC,

                    CASE WHEN @SortBy = 'Amount' AND @SortOrder = 'ASC' THEN t.Amount END ASC,
                    CASE WHEN @SortBy = 'Amount' AND @SortOrder = 'DESC' THEN t.Amount END DESC,

                    CASE WHEN @SortBy = 'Type' AND @SortOrder = 'ASC' THEN t.Type END ASC,
                    CASE WHEN @SortBy = 'Type' AND @SortOrder = 'DESC' THEN t.Type END DESC,

                    CASE WHEN @SortBy = 'Status' AND @SortOrder = 'ASC' THEN t.Status END ASC,
                    CASE WHEN @SortBy = 'Status' AND @SortOrder = 'DESC' THEN t.Status END DESC,

                    CASE WHEN @SortBy = 'IsHighValue' AND @SortOrder = 'ASC' THEN t.IsHighValue END ASC,
                    CASE WHEN @SortBy = 'IsHighValue' AND @SortOrder = 'DESC' THEN t.IsHighValue END DESC
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
