using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Branch_usp_GetBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_GetBranches]
                @BranchId INT = NULL,
                @BranchName NVARCHAR(100) = NULL,
                @IFSCCode NVARCHAR(50) = NULL,
                @City NVARCHAR(100) = NULL,
                @State NVARCHAR(100) = NULL,
                @Country NVARCHAR(100) = NULL,
                @Pincode NVARCHAR(20) = NULL,

                @SearchText NVARCHAR(100) = NULL,

                @CreatedFrom DATETIME = NULL,
                @CreatedTo DATETIME = NULL,

                @UpdatedFrom DATETIME = NULL,
                @UpdatedTo DATETIME = NULL,

                @SortBy NVARCHAR(50) = 'BranchName',
                @SortOrder NVARCHAR(10) = 'ASC',

                @Limit INT = 20,
                @Offset INT = 0
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    BranchId,
                    BranchName,
                    IFSCCode,
                    City,
                    State,
                    Country,
                    Pincode,
                    CreatedAt,
                    UpdatedAt,
                    COUNT(*) OVER() AS TotalCount
                FROM t_Branch
                WHERE
                    (@BranchId IS NULL OR BranchId = @BranchId)
                    AND (@BranchName IS NULL OR BranchName LIKE '%' + @BranchName + '%')
                    AND (@IFSCCode IS NULL OR IFSCCode LIKE '%' + @IFSCCode + '%')
                    AND (@City IS NULL OR City = @City)
                    AND (@State IS NULL OR State = @State)
                    AND (@Country IS NULL OR Country = @Country)
                    AND (@Pincode IS NULL OR Pincode = @Pincode)

                    AND (@SearchText IS NULL OR
                            BranchName LIKE '%' + @SearchText + '%' OR
                            IFSCCode LIKE '%' + @SearchText + '%' OR
                            City LIKE '%' + @SearchText + '%')

                    AND (@CreatedFrom IS NULL OR CreatedAt >= @CreatedFrom)
                    AND (@CreatedTo IS NULL OR CreatedAt <= @CreatedTo)

                    AND (@UpdatedFrom IS NULL OR UpdatedAt >= @UpdatedFrom)
                    AND (@UpdatedTo IS NULL OR UpdatedAt <= @UpdatedTo)

                ORDER BY
                    CASE WHEN @SortBy = 'BranchName' AND @SortOrder = 'ASC' THEN BranchName END ASC,
                    CASE WHEN @SortBy = 'BranchName' AND @SortOrder = 'DESC' THEN BranchName END DESC,

                    CASE WHEN @SortBy = 'IFSCCode' AND @SortOrder = 'ASC' THEN IFSCCode END ASC,
                    CASE WHEN @SortBy = 'IFSCCode' AND @SortOrder = 'DESC' THEN IFSCCode END DESC,

                    CASE WHEN @SortBy = 'City' AND @SortOrder = 'ASC' THEN City END ASC,
                    CASE WHEN @SortBy = 'City' AND @SortOrder = 'DESC' THEN City END DESC,

                    CASE WHEN @SortBy = 'State' AND @SortOrder = 'ASC' THEN State END ASC,
                    CASE WHEN @SortBy = 'State' AND @SortOrder = 'DESC' THEN State END DESC,

                    CASE WHEN @SortBy = 'Country' AND @SortOrder = 'ASC' THEN Country END ASC,
                    CASE WHEN @SortBy = 'Country' AND @SortOrder = 'DESC' THEN Country END DESC,

                    CASE WHEN @SortBy = 'Pincode' AND @SortOrder = 'ASC' THEN Pincode END ASC,
                    CASE WHEN @SortBy = 'Pincode' AND @SortOrder = 'DESC' THEN Pincode END DESC,

                    CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'ASC' THEN CreatedAt END ASC,
                    CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'DESC' THEN CreatedAt END DESC,

                    CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'ASC' THEN UpdatedAt END ASC,
                    CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'DESC' THEN UpdatedAt END DESC

                OFFSET @Offset ROWS
                FETCH NEXT @Limit ROWS ONLY;
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_GetBranches]");
        }
    }
}
