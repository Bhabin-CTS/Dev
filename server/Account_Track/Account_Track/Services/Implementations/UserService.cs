// File: Account_Track/Services/Implementations/UserService.cs
using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.Dtos.UserDto;
using Account_Track.Model;
using Account_Track.Services.Interfaces;
using Account_Track.Utils.Enum;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Account_Track.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        public UserService(ApplicationDbContext db) => _db = db;

        // -------------------- Helpers --------------------

        private static UserRole ParseRoleStrict(string role)
        {
            if (Enum.TryParse<UserRole>(role, ignoreCase: true, out var r)) return r;
            throw new ArgumentException("role must be one of: Officer, Manager, Admin", nameof(role));
        }

        /// <summary>
        /// rolesParam: comma-separated role tokens, numeric or text. "all" or "*" -> null (ignore filter).
        /// Invalid role tokens are ignored; if none valid, returns null to ignore filter.
        /// </summary>
        private static List<UserRole>? ParseRoleFilter(string? rolesParam)
        {
            if (string.IsNullOrWhiteSpace(rolesParam)) return null;

            var norm = rolesParam.Trim();
            if (norm.Equals("all", StringComparison.OrdinalIgnoreCase) || norm == "*")
                return null;

            var tokens = norm.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var list = new List<UserRole>();

            foreach (var t in tokens)
            {
                if (Enum.TryParse<UserRole>(t, ignoreCase: true, out var r))
                {
                    list.Add(r);
                    continue;
                }
                if (int.TryParse(t, out var ri) && Enum.IsDefined(typeof(UserRole), ri))
                {
                    list.Add((UserRole)ri);
                }
            }

            return list.Count == 0 ? null : list.Distinct().ToList();
        }

        private static (int? status, bool? isLocked) ParseStatusForUpdate(string status)
        {
            if (status.Equals("Locked", StringComparison.OrdinalIgnoreCase)) return (null, true);
            if (status.Equals("Active", StringComparison.OrdinalIgnoreCase)) return ((int)UserStatus.Active, false);
            if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)) return ((int)UserStatus.Inactive, false);
            throw new ArgumentException("status must be one of: Active, Inactive, Locked", nameof(status));
        }

        private static string? ToIso(DateTime? dt) => dt?.ToUniversalTime().ToString("o");

        private static string DisplayStatus(t_User u) => u.IsLocked ? "Locked" : u.Status.ToString();

        private static UserResponse Map(t_User u) => new()
        {
            UserId = u.UserId,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role.ToString(),
            BranchId = u.BranchId,
            Status = DisplayStatus(u),
            CreatedAt = ToIso(u.CreatedAt),
            UpdatedAt = ToIso(u.UpdatedAt)
        };

        private static string HashPasswordSha256(string input)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        // -------------------- Methods --------------------

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest dto, int userId)
        {
            // Default password = SHA256(email) (hex uppercase)
            var passwordHash = HashPasswordSha256(dto.Email);
            var roleEnum = ParseRoleStrict(dto.Role);

            // Expect exactly 1 entity from SP
            var list = await _db.Users
                .FromSqlInterpolated($@"
                    EXEC dbo.usp_User_Create
                        @Name={dto.Name},
                        @Email={dto.Email},
                        @Role={(int)roleEnum},
                        @BranchId={dto.BranchId},
                        @PasswordHash={passwordHash},
                        @PerformedByUserId={userId},
                        @LoginId={userId}
                ")
                .AsNoTracking()
                .ToListAsync();

            if (list.Count != 1)
                throw new ArgumentException("Create SP did not return a single row");

            return Map(list[0]);
        }

        public async Task<UserResponse> UpdateUserAsync(int targetUserId, UpdateUserRequest dto, int userId)
        {
            // Fetch current user for default password reset to hash(email)
            var existing = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == targetUserId);
            if (existing is null)
                throw new KeyNotFoundException("USER_NOT_FOUND");

            int? role = null;
            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                role = (int)ParseRoleStrict(dto.Role);
            }

            var passwordHash = HashPasswordSha256(existing.Email);

            var list = await _db.Users
                .FromSqlInterpolated($@"
                    EXEC dbo.usp_User_Update
                        @UserId={targetUserId},
                        @Name={dto.Name},
                        @Role={role},
                        @BranchId={dto.BranchId},
                        @PasswordHash={passwordHash}, -- reset to default = hash(email)
                        @PerformedByUserId={userId},
                        @LoginId={userId}
                ")
                .AsNoTracking()
                .ToListAsync();

            if (list.Count == 0)
                throw new KeyNotFoundException("USER_NOT_FOUND");
            if (list.Count != 1)
                throw new ArgumentException("Update SP did not return a single row");

            return Map(list[0]);
        }

        public async Task<UserResponse> UpdateUserStatusAsync(int targetUserId, ChangeUserStatusRequest dto, int userId)
        {
            var (status, isLocked) = ParseStatusForUpdate(dto.Status);

            var list = await _db.Users
                .FromSqlInterpolated($@"
                    EXEC dbo.usp_User_UpdateStatus
                        @UserId={targetUserId},
                        @Status={status},
                        @IsLocked={isLocked},
                        @Reason={dto.Reason},
                        @PerformedByUserId={userId},
                        @LoginId={userId}
                ")
                .AsNoTracking()
                .ToListAsync();

            if (list.Count == 0)
                throw new KeyNotFoundException("USER_NOT_FOUND");
            if (list.Count != 1)
                throw new ArgumentException("UpdateStatus SP did not return a single row");

            return Map(list[0]);
        }

        public async Task<(List<UserResponse> Data, PaginationDto Pagination)> GetUsersAsync(
            int? branchId, string? role, string? status, string? searchTerm,
            string? sortBy, string? sortOrder, int limit, int offset, int userId)
        {
            if (limit <= 0 || limit > 500 || offset < 0)
                throw new ArgumentException("Invalid pagination parameters");

            var q = _db.Users.AsNoTracking().AsQueryable();

            if (branchId.HasValue)
                q = q.Where(u => u.BranchId == branchId.Value);

            // role filter (tolerant)
            var roleFilter = ParseRoleFilter(role);
            if (roleFilter is not null && roleFilter.Count > 0)
                q = q.Where(u => roleFilter.Contains(u.Role));

            // status filter: allow all/* or numeric
            if (!string.IsNullOrWhiteSpace(status))
            {
                var s = status.Trim();
                if (!s.Equals("all", StringComparison.OrdinalIgnoreCase) && s != "*")
                {
                    if (string.Equals(s, "Locked", StringComparison.OrdinalIgnoreCase))
                    {
                        q = q.Where(u => u.IsLocked);
                    }
                    else if (string.Equals(s, "Active", StringComparison.OrdinalIgnoreCase) || s == "1")
                    {
                        q = q.Where(u => !u.IsLocked && u.Status == UserStatus.Active);
                    }
                    else if (string.Equals(s, "Inactive", StringComparison.OrdinalIgnoreCase) || s == "2")
                    {
                        q = q.Where(u => !u.IsLocked && u.Status == UserStatus.Inactive);
                    }
                    else
                    {
                        throw new ArgumentException("status must be one of: Active, Inactive, Locked", nameof(status));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.Trim();
                q = q.Where(u => u.Name.Contains(s) || u.Email.Contains(s));
            }

            bool desc = string.Equals(sortOrder, "DESC", StringComparison.OrdinalIgnoreCase);
            q = (sortBy?.ToLowerInvariant()) switch
            {
                "name" => desc ? q.OrderByDescending(u => u.Name) : q.OrderBy(u => u.Name),
                "email" => desc ? q.OrderByDescending(u => u.Email) : q.OrderBy(u => u.Email),
                "createdat" => desc ? q.OrderByDescending(u => u.CreatedAt) : q.OrderBy(u => u.CreatedAt),
                _ => desc ? q.OrderByDescending(u => u.Name) : q.OrderBy(u => u.Name)
            };

            var total = await q.CountAsync();
            var items = await q.Skip(offset).Take(limit).ToListAsync();

            var data = items.Select(Map).ToList();
            var pagination = new PaginationDto
            {
                Total = total,
                Limit = limit,
                Offset = offset
            };

            return (data, pagination);
        }

        public async Task<UserResponse> GetUserByIdAsync(int targetUserId, int userId)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == targetUserId);
            if (u is null)
                throw new KeyNotFoundException("USER_NOT_FOUND");

            return Map(u);
        }

        public async Task<UserResponse?> GetUserByEmailAsync(string email, int userId)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            return u is null ? null : Map(u);
        }
    }
}