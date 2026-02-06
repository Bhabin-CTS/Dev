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
            // 1) Drop if exists (EF can't use GO; run as separate statements)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.usp_Branch_Create', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Branch_Create;
");

            // 2) Create procedure
            migrationBuilder.Sql(@"
CREATE PROCEDURE dbo.usp_Branch_Create
    @BranchName NVARCHAR(100),
    @IFSCCode   NVARCHAR(50),
    @City       NVARCHAR(500),
    @State      NVARCHAR(100),
    @Country    NVARCHAR(100),
    @Pincode    NVARCHAR(20),
    @PerformedByUserId INT,
    @LoginId    INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF EXISTS (SELECT 1 FROM dbo.t_Branch WHERE IFSCCode = @IFSCCode)
            THROW 50010, 'DUPLICATE_IFSC', 1;

        INSERT INTO dbo.t_Branch
        (
            BranchName, IFSCCode, City, State, Country, Pincode,
            CreatedAt, UpdatedAt
        )
        VALUES
        (
            @BranchName, @IFSCCode, @City, @State, @Country, @Pincode,
            SYSUTCDATETIME(), SYSUTCDATETIME()
        );

        DECLARE @NewBranchId INT = SCOPE_IDENTITY();

        DECLARE @Before NVARCHAR(MAX) = N'{}';
        DECLARE @After  NVARCHAR(MAX);
        SELECT @After = (
            SELECT b.BranchId, b.BranchName, b.IFSCCode, b.City, b.State, b.Country, b.Pincode, b.CreatedAt, b.UpdatedAt
            FROM dbo.t_Branch b
            WHERE b.BranchId = @NewBranchId
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        INSERT INTO dbo.t_AuditLog
        (
            UserId, LoginId, EntityType, EntityId, Action, beforeState, afterState, CreatedAt
        )
        VALUES
        (
            @PerformedByUserId, @LoginId, N'Branch', @NewBranchId, N'CREATE', @Before, @After, SYSUTCDATETIME()
        );

        COMMIT TRAN;

        SELECT TOP 1 * FROM dbo.t_Branch WHERE BranchId = @NewBranchId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRAN;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(), @ErrNum INT = ERROR_NUMBER();
        THROW @ErrNum, @ErrMsg, 1;
    END CATCH
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.usp_Branch_Create', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Branch_Create;
");
        }
    }
}