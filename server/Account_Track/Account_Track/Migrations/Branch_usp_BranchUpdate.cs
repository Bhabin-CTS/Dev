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
            // 1) Drop if exists (separate call; EF doesn't support GO)
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.usp_Branch_Update', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Branch_Update;
");

            // 2) Create procedure (full definition)
            migrationBuilder.Sql(@"
CREATE PROCEDURE dbo.usp_Branch_Update
    @BranchId   INT,
    @BranchName NVARCHAR(150) = NULL,
    @IFSCCode   NVARCHAR(50)  = NULL,
    @City       NVARCHAR(500) = NULL,
    @State      NVARCHAR(100) = NULL,
    @Country    NVARCHAR(100) = NULL,
    @Pincode    NVARCHAR(20)  = NULL,
    @PerformedByUserId INT,
    @LoginId    INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM dbo.t_Branch WHERE BranchId = @BranchId)
            THROW 50011, 'BRANCH_NOT_FOUND', 1;

        IF @IFSCCode IS NOT NULL
           AND EXISTS (SELECT 1 FROM dbo.t_Branch WHERE IFSCCode = @IFSCCode AND BranchId <> @BranchId)
            THROW 50010, 'DUPLICATE_IFSC', 1;

        DECLARE @Before NVARCHAR(MAX);
        SELECT @Before = (
            SELECT b.BranchId, b.BranchName, b.IFSCCode, b.City, b.State, b.Country, b.Pincode, b.CreatedAt, b.UpdatedAt
            FROM dbo.t_Branch b
            WHERE b.BranchId = @BranchId
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        UPDATE b
        SET
            BranchName = ISNULL(@BranchName, b.BranchName),
            IFSCCode   = ISNULL(@IFSCCode,   b.IFSCCode),
            City       = ISNULL(@City,       b.City),
            State      = ISNULL(@State,      b.State),
            Country    = ISNULL(@Country,    b.Country),
            Pincode    = ISNULL(@Pincode,    b.Pincode),
            UpdatedAt  = SYSUTCDATETIME()
        FROM dbo.t_Branch b
        WHERE b.BranchId = @BranchId;

        DECLARE @After NVARCHAR(MAX);
        SELECT @After = (
            SELECT b.BranchId, b.BranchName, b.IFSCCode, b.City, b.State, b.Country, b.Pincode, b.CreatedAt, b.UpdatedAt
            FROM dbo.t_Branch b
            WHERE b.BranchId = @BranchId
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        INSERT INTO dbo.t_AuditLog
        (
            UserId, LoginId, EntityType, EntityId, Action, beforeState, afterState, CreatedAt
        )
        VALUES
        (
            @PerformedByUserId, @LoginId, N'Branch', @BranchId, N'UPDATE', @Before, @After, SYSUTCDATETIME()
        );

        COMMIT TRAN;

        SELECT TOP 1 * FROM dbo.t_Branch WHERE BranchId = @BranchId;
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
IF OBJECT_ID(N'dbo.usp_Branch_Update', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Branch_Update;
");
        }
    }
}