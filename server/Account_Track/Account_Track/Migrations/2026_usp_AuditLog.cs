using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    public partial class usp_AuditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE dbo.usp_AuditLog
            (
                @Action        VARCHAR(20),

                -- Filters
                @AuditLogId    INT = NULL,
                @UserId        INT = NULL,
                @LoginId       INT = NULL,
                @EntityType    NVARCHAR(100) = NULL,
                @EntityId      INT = NULL,
                @AuditAction   NVARCHAR(100) = NULL,

                -- Search
                @SearchText    NVARCHAR(200) = NULL,

                -- Date filters
                @FromUtc       DATETIME2 = NULL,
                @ToUtc         DATETIME2 = NULL,

                -- Sorting & Paging
                @SortBy        NVARCHAR(50) = 'CreatedAt',
                @SortOrder     NVARCHAR(4)  = 'DESC',
                @Limit         INT = 20,
                @Offset        INT = 0
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                ------------------------------------------------------------
                -- LIST
                ------------------------------------------------------------
                IF @Action = 'LIST'
                BEGIN
                    SELECT
                        a.AuditLogId,
                        a.UserId,
                        a.LoginId,
                        a.EntityType,
                        a.EntityId,
                        a.Action,
                        a.beforeState,
                        a.afterState,
                        a.CreatedAt,
                        u.Name AS ChangedByName,
                        CAST(u.Role AS INT) AS ChangedByRoleId,
                        COUNT(*) OVER() AS TotalCount
                    FROM t_AuditLog a
                    LEFT JOIN t_User u ON u.UserId = a.UserId
                    WHERE
                        (@UserId IS NULL OR a.UserId = @UserId)
                        AND (@LoginId IS NULL OR a.LoginId = @LoginId)
                        AND (@EntityType IS NULL OR a.EntityType = @EntityType)
                        AND (@EntityId IS NULL OR a.EntityId = @EntityId)
                        AND (@AuditAction IS NULL OR a.Action = @AuditAction)
                        AND (@FromUtc IS NULL OR a.CreatedAt >= @FromUtc)
                        AND (@ToUtc IS NULL OR a.CreatedAt <= @ToUtc)

                        -- 🔎 Search filter
                        AND (
                            @SearchText IS NULL OR
                            a.EntityType LIKE '%' + @SearchText + '%' OR
                            a.Action LIKE '%' + @SearchText + '%' OR
                            u.Name LIKE '%' + @SearchText + '%'
                        )

                    ORDER BY
                        CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'ASC' THEN a.CreatedAt END ASC,
                        CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'DESC' THEN a.CreatedAt END DESC,

                        CASE WHEN @SortBy = 'EntityType' AND @SortOrder = 'ASC' THEN a.EntityType END ASC,
                        CASE WHEN @SortBy = 'EntityType' AND @SortOrder = 'DESC' THEN a.EntityType END DESC,

                        CASE WHEN @SortBy = 'Action' AND @SortOrder = 'ASC' THEN a.Action END ASC,
                        CASE WHEN @SortBy = 'Action' AND @SortOrder = 'DESC' THEN a.Action END DESC,

                        a.AuditLogId DESC

                    OFFSET @Offset ROWS
                    FETCH NEXT @Limit ROWS ONLY;

                    RETURN;
                END

                ------------------------------------------------------------
                -- GET BY ID
                ------------------------------------------------------------
                IF @Action = 'GET_BY_ID'
                BEGIN
                    SELECT
                        a.AuditLogId,
                        a.UserId,
                        a.LoginId,
                        a.EntityType,
                        a.EntityId,
                        a.Action,
                        a.beforeState,
                        a.afterState,
                        a.CreatedAt,
                        u.Name AS ChangedByName,
                        CAST(u.Role AS INT) AS ChangedByRoleId
                    FROM t_AuditLog a
                    LEFT JOIN t_User u ON u.UserId = a.UserId
                    WHERE a.AuditLogId = @AuditLogId;

                    RETURN;
                END
            END
            ";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID('dbo.usp_AuditLog', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_AuditLog;");
        }
    }
}