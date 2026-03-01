using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.DTOs.AuditLogDto;
using Account_Track.Utils.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Account_Track.Services.Implementations
{
    public class AuditLogServiceSp : IAuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogServiceSp(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LIST AUDIT LOGS USING usp_AuditLog -> LIST
        // ============================================================
        public async Task<(List<AuditLogDto> Items, PaginationDto Pagination)> GetAsync(AuditLogQueryDto query)
        {
            var sql = @"
                EXEC dbo.usp_AuditLog
                    @Action = @Action,
                    @AuditLogId = @AuditLogId,
                    @UserId = @UserId,
                    @LoginId = @LoginId,
                    @EntityType = @EntityType,
                    @EntityId = @EntityId,
                    @AuditAction = @AuditAction,
                    @SearchText = @SearchText,
                    @FromUtc = @FromUtc,
                    @ToUtc = @ToUtc,
                    @SortBy = @SortBy,
                    @SortOrder = @SortOrder,
                    @Limit = @Limit,
                    @Offset = @Offset";

            var parameters = new[]
            {
                new SqlParameter("@Action", "LIST"),
                new SqlParameter("@AuditLogId", DBNull.Value),
                new SqlParameter("@UserId", query.UserId ?? (object)DBNull.Value),
                new SqlParameter("@LoginId", query.LoginId ?? (object)DBNull.Value),
                new SqlParameter("@EntityType", query.EntityType ?? (object)DBNull.Value),
                new SqlParameter("@EntityId", query.EntityId ?? (object)DBNull.Value),
                new SqlParameter("@AuditAction", query.Action ?? (object)DBNull.Value),
                new SqlParameter("@SearchText", query.SearchText ?? (object)DBNull.Value),
                new SqlParameter("@FromUtc", query.FromUtc ?? (object)DBNull.Value),
                new SqlParameter("@ToUtc", query.ToUtc ?? (object)DBNull.Value),
                new SqlParameter("@SortBy", query.SortBy ?? "CreatedAt"),
                new SqlParameter("@SortOrder", query.SortOrder ?? "DESC"),
                new SqlParameter("@Limit", query.Limit),
                new SqlParameter("@Offset", query.Offset)
            };

            var spResult = await _context.Database
                .SqlQueryRaw<AuditLogSpResultDto>(sql, parameters)
                .ToListAsync();

            int total = spResult.FirstOrDefault()?.TotalCount ?? 0;

            var data = spResult.Select(x =>
            {
                string? roleName = Enum.IsDefined(typeof(UserRole), x.ChangedByRoleId)
                    ? ((UserRole)x.ChangedByRoleId).ToString()
                    : null;

                return new AuditLogDto
                {
                    AuditLogId = x.AuditLogId,
                    UserId = x.UserId,
                    LoginId = x.LoginId,
                    EntityType = x.EntityType,
                    EntityId = x.EntityId,
                    Action = x.Action,
                    BeforeState = x.beforeState,
                    AfterState = x.afterState,
                    CreatedAt = x.CreatedAt,
                    ChangedByName = x.ChangedByName,
                    ChangedByRole = roleName
                };
            }).ToList();

            return (data, new PaginationDto
            {
                Total = total,
                Limit = query.Limit,
                Offset = query.Offset
            });
        }


        // ============================================================
        // GET BY ID 
        // ============================================================
        public async Task<AuditLogDto?> GetByIdSpAsync(int id)
        {
            var sql = @"
                EXEC dbo.usp_AuditLog
                    @Action = @Action,
                    @AuditLogId = @AuditLogId";

            var parameters = new[]
            {
                new SqlParameter("@Action", "GET_BY_ID"),
                new SqlParameter("@AuditLogId", id)
            };

            var result = await _context.Database
                .SqlQueryRaw<AuditLogGetByIdSpResultDto>(sql, parameters)
                .ToListAsync();

            var item = result.FirstOrDefault();
            if (item == null)
                return null;

            string? roleName = Enum.IsDefined(typeof(UserRole), item.ChangedByRoleId)
                ? ((UserRole)item.ChangedByRoleId).ToString()
                : null;

            return new AuditLogDto
            {
                AuditLogId = item.AuditLogId,
                UserId = item.UserId,
                LoginId = item.LoginId,
                EntityType = item.EntityType,
                EntityId = item.EntityId,
                Action = item.Action,
                BeforeState = item.beforeState,
                AfterState = item.afterState,
                CreatedAt = item.CreatedAt,
                ChangedByName = item.ChangedByName,
                ChangedByRole = roleName
            };
        }

    }
}