using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Approval_usp_GetApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE [dbo].[usp_GetApprovals]
    @AccountId     INT           = NULL,
    @ReviewerId    INT           = NULL,
    @Decision      VARCHAR(50)   = NULL,
    @Type          VARCHAR(50)   = NULL,
    @MinAmount     DECIMAL(18,2) = NULL,
    @MaxAmount     DECIMAL(18,2) = NULL,
    @FromDate      DATE          = NULL,
    @ToDate        DATE          = NULL,
    @Limit         INT           = 20,
    @Offset        INT           = 0,
    @SortBy        SYSNAME       = NULL,  -- whitelist enforced below
    @SortDir       VARCHAR(4)    = NULL,  -- 'ASC' | 'DESC'
    @UserId        INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserBranchId INT;
    DECLARE @UserRole INT;

    -- Adjust to your actual role values if different
    DECLARE @ROLE_ADMIN   INT = 3;
    DECLARE @ROLE_MANAGER INT = 2;
    DECLARE @ROLE_OFFICER INT = 1;

    SELECT 
        @UserBranchId = U.BranchId,
        @UserRole     = U.[Role]
    FROM dbo.t_User AS U
    WHERE U.UserId = @UserId;

    -- Safe sort column/dir
    DECLARE @SortColumn NVARCHAR(100) =
        CASE UPPER(ISNULL(@SortBy,'TRANSACTIONDATE'))
            WHEN 'TRANSACTIONDATE' THEN 'TransactionDate'
            WHEN 'AMOUNT'          THEN 'Amount'
            WHEN 'APPROVALID'      THEN 'ApprovalId'
            ELSE 'TransactionDate'
        END;

    DECLARE @SortDirection NVARCHAR(4) =
        CASE UPPER(ISNULL(@SortDir,'DESC'))
            WHEN 'ASC'  THEN 'ASC'
            WHEN 'DESC' THEN 'DESC'
            ELSE 'DESC'
        END;

    ;WITH Base AS
    (
        SELECT
            A.ApprovalId,
            A.TransactionId,
            A.Decision,
            A.Comments,
            T.FromAccountId AS AccountId,
            T.[Type],
            T.Amount,
            T.CreatedAt AS TransactionDate,
            A.ReviewerId,
            U.[Name] AS ReviewerName,
            CAST(U.[Role] AS INT) AS ReviewerRole,
            T.BranchId
        FROM dbo.t_Approval AS A
        JOIN dbo.t_Transaction AS T
            ON T.TransactionID = A.TransactionId
        JOIN dbo.t_User AS U
            ON U.UserId = A.ReviewerId
        WHERE
            (@AccountId  IS NULL OR T.FromAccountId = @AccountId OR T.ToAccountId = @AccountId)
            AND (@ReviewerId IS NULL OR A.ReviewerId = @ReviewerId)
            AND (@Decision  IS NULL OR A.Decision = @Decision)
            AND (@Type      IS NULL OR T.[Type] = @Type)
            AND (@MinAmount IS NULL OR T.Amount >= @MinAmount)
            AND (@MaxAmount IS NULL OR T.Amount <= @MaxAmount)
            AND (@FromDate  IS NULL OR T.CreatedAt >= @FromDate)
            AND (@ToDate    IS NULL OR T.CreatedAt < DATEADD(DAY, 1, @ToDate)) -- inclusive end date

            -- SECURITY: Admin sees all, others limited to their Branch
            AND (
                @UserRole = @ROLE_ADMIN
                OR T.BranchId = @UserBranchId
            )
    )
    SELECT
        b.ApprovalId,
        b.TransactionId,
        b.Decision,
        b.Comments,
        b.AccountId,
        b.[Type],
        b.Amount,
        b.TransactionDate,
        b.ReviewerId,
        b.ReviewerName,
        b.ReviewerRole,
        b.BranchId,
        COUNT(*) OVER() AS TotalCount
    FROM Base b
    ORDER BY
        CASE WHEN @SortColumn = 'TransactionDate' AND @SortDirection = 'ASC'  THEN b.TransactionDate END ASC,
        CASE WHEN @SortColumn = 'Amount'          AND @SortDirection = 'ASC'  THEN b.Amount END ASC,
        CASE WHEN @SortColumn = 'ApprovalId'      AND @SortDirection = 'ASC'  THEN b.ApprovalId END ASC,
        CASE WHEN @SortColumn = 'TransactionDate' AND @SortDirection = 'DESC' THEN b.TransactionDate END DESC,
        CASE WHEN @SortColumn = 'Amount'          AND @SortDirection = 'DESC' THEN b.Amount END DESC,
        CASE WHEN @SortColumn = 'ApprovalId'      AND @SortDirection = 'DESC' THEN b.ApprovalId END DESC
    OFFSET @Offset ROWS
    FETCH NEXT @Limit ROWS ONLY;

    -- OPTION(RECOMPILE); -- if you hit parameter sniffing issues
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
