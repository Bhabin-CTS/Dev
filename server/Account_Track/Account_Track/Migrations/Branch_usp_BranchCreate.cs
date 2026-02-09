using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Branch_usp_BranchCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_Branch_Create]
                @BranchName NVARCHAR(100),
                @IFSCCode NVARCHAR(50),
                @City NVARCHAR(100),
                @State NVARCHAR(100),
                @Country NVARCHAR(100),
                @Pincode NVARCHAR(20),
                @UserId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                IF EXISTS (SELECT 1 FROM t_Branch WHERE IFSCCode = @IFSCCode)
                    THROW 50010, 'IFSCCode already exists', 1;

                INSERT INTO t_Branch
                (BranchName, IFSCCode, City, State, Country, Pincode, CreatedAt)
                VALUES
                (@BranchName, @IFSCCode, @City, @State, @Country, @Pincode, GETUTCDATE());

                SELECT *
                FROM t_Branch
                WHERE BranchId = SCOPE_IDENTITY();
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_Branch_Create]");
        }
    }
}