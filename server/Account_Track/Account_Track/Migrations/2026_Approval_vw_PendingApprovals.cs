using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Approval_vw_PendingApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW dbo.vw_PendingApprovals
AS
SELECT
    A.ApprovalId,
    A.TransactionId      AS TransactionID,
    T.FromAccountId      AS AccountID,
    T.[Type]             AS [Type],
    T.Amount             AS Amount,
    A.ReviewerId         AS ReviewerID,
    A.Decision           AS Decision
FROM dbo.t_Approval AS A
JOIN dbo.t_Transaction AS T
    ON T.TransactionID = A.TransactionId
-- Only pending decisions (DecisionType.Pending = 1)
WHERE A.Decision = 1;");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.vw_PendingApprovals', 'V') IS NOT NULL
    DROP VIEW dbo.vw_PendingApprovals;
");

        }
    }
}
