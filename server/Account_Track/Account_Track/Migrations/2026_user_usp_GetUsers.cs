using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class user_usp_GetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_GetUsers]
                @BranchId INT = NULL,
                @Role INT = NULL,
                @Status INT = NULL,
                @IsLocked BIT = NULL,
                @Search NVARCHAR(100) = NULL,
                @CreatedFrom DATETIME = NULL,
                @CreatedTo DATETIME = NULL,
                @UpdatedFrom DATETIME = NULL,
                @UpdatedTo DATETIME = NULL,
                @SortBy NVARCHAR(50) = 'Name',
                @SortOrder NVARCHAR(10) = 'ASC',
                @Limit INT = 20,
                @Offset INT = 0
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    UserId,
                    Name,
                    Email,
                    Role,
                    BranchId,
                    Status,
                    IsLocked,
                    CreatedAt,
                    UpdatedAt,
                    COUNT(*) OVER() AS TotalCount
                FROM t_User
                WHERE
                    (@BranchId IS NULL OR BranchId = @BranchId)
                    AND (@Role IS NULL OR Role = @Role)
                    AND (@Status IS NULL OR Status = @Status)
                    AND (@IsLocked IS NULL OR IsLocked = @IsLocked)
                    AND (@Search IS NULL OR 
                            Name LIKE '%' + @Search + '%' OR 
                            Email LIKE '%' + @Search + '%')
                    AND (@CreatedFrom IS NULL OR CreatedAt >= @CreatedFrom)
                    AND (@CreatedTo IS NULL OR CreatedAt <= @CreatedTo)
                    AND (@UpdatedFrom IS NULL OR UpdatedAt >= @UpdatedFrom)
                    AND (@UpdatedTo IS NULL OR UpdatedAt <= @UpdatedTo)

                ORDER BY
                    -- Name sorting
                    CASE WHEN @SortBy = 'Name' AND @SortOrder = 'ASC' THEN Name END ASC,
                    CASE WHEN @SortBy = 'Name' AND @SortOrder = 'DESC' THEN Name END DESC,

                    -- Email sorting
                    CASE WHEN @SortBy = 'Email' AND @SortOrder = 'ASC' THEN Email END ASC,
                    CASE WHEN @SortBy = 'Email' AND @SortOrder = 'DESC' THEN Email END DESC,

                    -- CreatedAt sorting
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'ASC' THEN CreatedAt END ASC,
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'DESC' THEN CreatedAt END DESC,

                    -- UpdatedAt sorting
                    CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'ASC' THEN UpdatedAt END ASC,
                    CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'DESC' THEN UpdatedAt END DESC,

                    -- Role sorting
                    CASE WHEN @SortBy = 'Role' AND @SortOrder = 'ASC' THEN Role END ASC,
                    CASE WHEN @SortBy = 'Role' AND @SortOrder = 'DESC' THEN Role END DESC,

                    -- Status sorting
                    CASE WHEN @SortBy = 'Status' AND @SortOrder = 'ASC' THEN Status END ASC,
                    CASE WHEN @SortBy = 'Status' AND @SortOrder = 'DESC' THEN Status END DESC,

                    -- IsLocked sorting
                    CASE WHEN @SortBy = 'IsLocked' AND @SortOrder = 'ASC' THEN IsLocked END ASC,
                    CASE WHEN @SortBy = 'IsLocked' AND @SortOrder = 'DESC' THEN IsLocked END DESC

                OFFSET @Offset ROWS
                FETCH NEXT @Limit ROWS ONLY;
            END";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            @"IF OBJECT_ID('[dbo].[usp_GetUsers]', 'P') IS NOT NULL
              DROP PROCEDURE [dbo].[usp_GetUsers];");
        }
    }
}
