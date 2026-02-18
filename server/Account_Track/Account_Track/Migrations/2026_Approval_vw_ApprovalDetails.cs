using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Approval_vw_ApprovalDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW dbo.vw_ApprovalDetail
AS
SELECT
    A.ApprovalId,
    A.TransactionId,
    A.Decision,
    A.Comments,
    T.FromAccountId         AS AccountId,
    T.[Type]                AS [Type],
    T.Amount,
    T.CreatedAt             AS TransactionDate,
    A.ReviewerId,
    U.[Name]                AS ReviewerName,
    
CAST(U.[Role] AS int)   AS ReviewerRole
FROM dbo.t_Approval AS A
JOIN dbo.t_Transaction AS T
    ON T.TransactionID = A.TransactionId
JOIN dbo.t_User AS U
    ON U.UserId = A.ReviewerId;");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.vw_ApprovalDetail', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ApprovalDetail;
");

        }
    }
}
