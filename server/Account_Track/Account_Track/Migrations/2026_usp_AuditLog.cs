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
                @Action       VARCHAR(20),

                -- Shared filters
                @AuditLogId   INT = NULL,
                @UserId       INT = NULL,
                @LoginId      INT = NULL,
                @EntityType   NVARCHAR(100) = NULL,
                @EntityId     INT = NULL,
                @AuditAction  NVARCHAR(100) = NULL,

                -- Time filters
                @FromUtc      DATETIME2 = NULL,
                @ToUtc        DATETIME2 = NULL,

                -- Sorting
                @SortBy       NVARCHAR(50) = 'createdat',
                @SortDir      NVARCHAR(4)  = 'desc',

                -- Pagination
                @Limit        INT = 25,
                @Offset       INT = 0
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                -------------------------------------------------------------------
                -- LIST ACTION
                -------------------------------------------------------------------
                IF @Action = 'LIST'
                BEGIN
                    IF @Limit  < 1   SET @Limit = 25;
                    IF @Limit  > 100 SET @Limit = 100;
                    IF @Offset < 0   SET @Offset = 0;

                    SET @SortBy  = LOWER(@SortBy);
                    SET @SortDir = LOWER(@SortDir);
                    IF @SortDir NOT IN ('asc','desc') SET @SortDir = 'desc';

                    ;WITH F AS
                    (
                        SELECT 
                            a.AuditLogId,
                            a.UserId,
                            a.LoginId,
                            a.EntityType,
                            a.EntityId,
                            a.Action,
                            a.beforeState,
                            a.afterState,
                            a.CreatedAt
                        FROM t_AuditLog a
                        WHERE 
                            (@UserId     IS NULL OR a.UserId = @UserId)
                            AND (@LoginId    IS NULL OR a.LoginId = @LoginId)
                            AND (@EntityType IS NULL OR a.EntityType = @EntityType)
                            AND (@EntityId   IS NULL OR a.EntityId   = @EntityId)
                            AND (@AuditAction IS NULL OR a.Action    = @AuditAction)
                            AND (@FromUtc     IS NULL OR a.CreatedAt >= @FromUtc)
                            AND (@ToUtc       IS NULL OR a.CreatedAt <= @ToUtc)
                    ),
                    X AS
                    (
                        SELECT
                            F.*,
                            u.Name              AS ChangedByName,
                            CAST(u.Role AS INT) AS ChangedByRoleId,
                            COUNT(*) OVER()     AS TotalCount,
                            ROW_NUMBER() OVER
                            (
                                ORDER BY
                                    CASE WHEN @SortBy = 'createdat'  AND @SortDir = 'asc'  THEN F.CreatedAt END ASC,
                                    CASE WHEN @SortBy = 'createdat'  AND @SortDir = 'desc' THEN F.CreatedAt END DESC,
                                    CASE WHEN @SortBy = 'entitytype' AND @SortDir = 'asc'  THEN F.EntityType END ASC,
                                    CASE WHEN @SortBy = 'entitytype' AND @SortDir = 'desc' THEN F.EntityType END DESC,
                                    CASE WHEN @SortBy = 'action'     AND @SortDir = 'asc'  THEN F.Action     END ASC,
                                    CASE WHEN @SortBy = 'action'     AND @SortDir = 'desc' THEN F.Action     END DESC,
                                    F.AuditLogId DESC
                            ) AS rn
                        FROM F
                        LEFT JOIN t_User u ON u.UserId = F.UserId
                    )
                    SELECT 
                        AuditLogId,
                        UserId,
                        LoginId,
                        EntityType,
                        EntityId,
                        Action,
                        beforeState,
                        afterState,
                        CreatedAt,
                        ChangedByName,
                        ChangedByRoleId,
                        TotalCount
                    FROM X
                    WHERE rn BETWEEN (@Offset + 1) AND (@Offset + @Limit);

                    RETURN;
                END

                -------------------------------------------------------------------
                -- GET_BY_ID ACTION
                -------------------------------------------------------------------
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
                    WHERE 
                        a.AuditLogId = @AuditLogId
                        AND (@AuditAction IS NULL OR a.Action = @AuditAction);

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