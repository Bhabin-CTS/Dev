using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Approval_vw_ApprovalAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW dbo.vw_ApprovalAudit
AS
SELECT
    A.ApprovalId,
    A.TransactionId                    AS TransactionID,
    U.[Name]                           AS ReviewerName,
    CAST(U.[Role] AS int)              AS ReviewerRole,
    A.Decision,
    ISNULL(A.DecidedAt, A.CreatedAt)   AS ApprovalDate,
    A.Comments
FROM dbo.t_Approval AS A
JOIN dbo.t_User AS U
    ON U.UserId = A.ReviewerId;");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.vw_ApprovalAudit', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ApprovalAudit;
");

        }
    }
}
