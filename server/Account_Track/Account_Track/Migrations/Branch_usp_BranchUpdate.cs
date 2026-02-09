using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Branch_usp_BranchUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_Branch_Update]
                @BranchId INT,
                @BranchName NVARCHAR(100) = NULL,
                @IFSCCode NVARCHAR(50) = NULL,
                @City NVARCHAR(100) = NULL,
                @State NVARCHAR(100) = NULL,
                @Country NVARCHAR(100) = NULL,
                @Pincode NVARCHAR(20) = NULL,
                @UserId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                IF NOT EXISTS (SELECT 1 FROM t_Branch WHERE BranchId = @BranchId)
                    THROW 50011, 'Branch not found', 1;

                UPDATE t_Branch
                SET
                    BranchName = ISNULL(@BranchName, BranchName),
                    IFSCCode = ISNULL(@IFSCCode, IFSCCode),
                    City = ISNULL(@City, City),
                    State = ISNULL(@State, State),
                    Country = ISNULL(@Country, Country),
                    Pincode = ISNULL(@Pincode, Pincode),
                    UpdatedAt = GETUTCDATE()
                WHERE BranchId = @BranchId;

                SELECT * FROM t_Branch WHERE BranchId = @BranchId;
            END

            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_Branch_Update]");
        }
    }
}