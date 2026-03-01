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
                @UserId INT,
                @LoginId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                IF EXISTS (SELECT 1 FROM t_Branch WHERE IFSCCode = @IFSCCode)
                    THROW 50010, 'IFSCCode already exists', 1;

                -------------------------------------------------------
                -- CREATE BRANCH
                -------------------------------------------------------
                INSERT INTO t_Branch
                (BranchName, IFSCCode, City, State, Country, Pincode, CreatedAt)
                VALUES
                (@BranchName, @IFSCCode, @City, @State, @Country, @Pincode, GETUTCDATE());

                DECLARE @NewBranchId INT = SCOPE_IDENTITY();

                -------------------------------------------------------
                -- CAPTURE AFTER STATE FOR AUDIT
                -------------------------------------------------------
                DECLARE @BranchAfterState NVARCHAR(MAX);
                SELECT @BranchAfterState = (
                    SELECT BranchId, BranchName, IFSCCode, City, State, Country, Pincode, CreatedAt, UpdatedAt
                    FROM t_Branch
                    WHERE BranchId = @NewBranchId
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                );

                -------------------------------------------------------
                -- AUDIT LOG (CREATE)
                -------------------------------------------------------
                INSERT INTO t_AuditLog
                (
                    UserId,
                    LoginId,
                    EntityType,
                    EntityId,
                    Action,
                    beforeState,
                    afterState,
                    CreatedAt
                )
                VALUES
                (
                    @UserId,
                    @LoginId,
                    'Branch',
                    @NewBranchId,
                    'CREATE',
                    NULL,
                    @BranchAfterState,
                    GETUTCDATE()
                );

                -------------------------------------------------------
                -- RETURN CREATED BRANCH
                -------------------------------------------------------
                SELECT *
                FROM t_Branch
                WHERE BranchId = @NewBranchId;
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