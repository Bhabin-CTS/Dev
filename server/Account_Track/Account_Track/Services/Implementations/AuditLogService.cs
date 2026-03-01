using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.Services;
using Account_Track.Utils;
using Account_Track.Utils.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var sqlConn = (SqlConnection)_context.Database.GetDbConnection();

            await using var cmd = new SqlCommand("dbo.usp_AuditLog", sqlConn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            // required action
            cmd.Parameters.AddWithValue("@Action", "LIST");

            // filters
            cmd.Parameters.AddWithValue("@AuditLogId", DBNull.Value);
            cmd.Parameters.AddWithValue("@AuditAction", (object?)query.Action ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)query.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LoginId", (object?)query.LoginId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EntityType", (object?)query.EntityType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EntityId", (object?)query.EntityId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FromUtc", (object?)query.FromUtc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ToUtc", (object?)query.ToUtc ?? DBNull.Value);

            // sorting + paging
            cmd.Parameters.AddWithValue("@SortBy", query.SortBy ?? "createdat");
            cmd.Parameters.AddWithValue("@SortDir", query.SortDir ?? "desc");
            cmd.Parameters.AddWithValue("@Limit", query.Limit);
            cmd.Parameters.AddWithValue("@Offset", query.Offset);

            bool wasClosed = sqlConn.State == System.Data.ConnectionState.Closed;
            if (wasClosed) await sqlConn.OpenAsync();

            var list = new List<AuditLogDto>();
            int total = 0;

            try
            {
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // convert enum id to name
                    int roleId = reader.IsDBNull(reader.GetOrdinal("ChangedByRoleId"))
                        ? -1
                        : reader.GetInt32(reader.GetOrdinal("ChangedByRoleId"));

                    string? roleName = Enum.IsDefined(typeof(UserRole), roleId)
                        ? ((UserRole)roleId).ToString()
                        : null;

                    list.Add(new AuditLogDto
                    {
                        AuditLogId = reader.GetInt32(reader.GetOrdinal("AuditLogId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        LoginId = reader.GetInt32(reader.GetOrdinal("LoginId")),
                        EntityType = reader.GetString(reader.GetOrdinal("EntityType")),
                        EntityId = reader.GetInt32(reader.GetOrdinal("EntityId")),
                        Action = reader.GetString(reader.GetOrdinal("Action")),
                        BeforeState = reader.IsDBNull(reader.GetOrdinal("beforeState"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("beforeState")),
                        AfterState = reader.GetString(reader.GetOrdinal("afterState")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        ChangedByName = reader.IsDBNull(reader.GetOrdinal("ChangedByName"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("ChangedByName")),
                        ChangedByRole = roleName
                    });

                    if (total == 0 && !reader.IsDBNull(reader.GetOrdinal("TotalCount")))
                        total = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                }
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    await sqlConn.CloseAsync();
            }

            return (list, new PaginationDto
            {
                Total = total,
                Limit = query.Limit,
                Offset = query.Offset
            });
        }

        // ============================================================
        // GET BY ID 
        // ============================================================
        public async Task<AuditLogDto?> GetByIdSpAsync(int id, string? action = null)
        {
            var sqlConn = (SqlConnection)_context.Database.GetDbConnection();

            await using var cmd = new SqlCommand("dbo.usp_AuditLog", sqlConn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Action", "GET_BY_ID");
            cmd.Parameters.AddWithValue("@AuditLogId", id);
            cmd.Parameters.AddWithValue("@AuditAction", (object?)action ?? DBNull.Value);

            // unused params required by SP
            cmd.Parameters.AddWithValue("@UserId", DBNull.Value);
            cmd.Parameters.AddWithValue("@LoginId", DBNull.Value);
            cmd.Parameters.AddWithValue("@EntityType", DBNull.Value);
            cmd.Parameters.AddWithValue("@EntityId", DBNull.Value);
            cmd.Parameters.AddWithValue("@FromUtc", DBNull.Value);
            cmd.Parameters.AddWithValue("@ToUtc", DBNull.Value);
            cmd.Parameters.AddWithValue("@SortBy", DBNull.Value);
            cmd.Parameters.AddWithValue("@SortDir", DBNull.Value);
            cmd.Parameters.AddWithValue("@Limit", DBNull.Value);
            cmd.Parameters.AddWithValue("@Offset", DBNull.Value);

            if (sqlConn.State == System.Data.ConnectionState.Closed)
                await sqlConn.OpenAsync();

            try
            {
                await using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return null;

                int roleId = reader.IsDBNull(reader.GetOrdinal("ChangedByRoleId"))
                    ? -1
                    : reader.GetInt32(reader.GetOrdinal("ChangedByRoleId"));

                string? roleName = Enum.IsDefined(typeof(UserRole), roleId)
                    ? ((UserRole)roleId).ToString()
                    : null;

                return new AuditLogDto
                {
                    AuditLogId = reader.GetInt32(reader.GetOrdinal("AuditLogId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    LoginId = reader.GetInt32(reader.GetOrdinal("LoginId")),
                    EntityType = reader.GetString(reader.GetOrdinal("EntityType")),
                    EntityId = reader.GetInt32(reader.GetOrdinal("EntityId")),
                    Action = reader.GetString(reader.GetOrdinal("Action")),
                    BeforeState = reader.IsDBNull(reader.GetOrdinal("beforeState"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("beforeState")),
                    AfterState = reader.GetString(reader.GetOrdinal("afterState")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    ChangedByName = reader.IsDBNull(reader.GetOrdinal("ChangedByName"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ChangedByName")),
                    ChangedByRole = roleName
                };
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    await sqlConn.CloseAsync();
            }
        }

       
    }
}