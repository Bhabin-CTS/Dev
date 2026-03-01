using Account_Track.Data;
using Account_Track.Dtos.UserDto;
using Account_Track.DTOs;
using Account_Track.DTOs.AuthDto;
using Account_Track.DTOs.UsersDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Account_Track.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto dto, int userId, int loginId)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Email);

            var sql = @"EXEC usp_User
                        @Action = @Action,
                        @Name = @Name,
                        @Email = @Email,
                        @Role = @Role,
                        @BranchId = @BranchId,
                        @PasswordHash = @PasswordHash,
                        @UserId = @UserId,
                        @LoginId = @LoginId";

            var parameters = new[]
            {
                new SqlParameter("@Action", "CREATE"),
                new SqlParameter("@Name", dto.Name),
                new SqlParameter("@Email", dto.Email),
                new SqlParameter("@Role", dto.Role),
                new SqlParameter("@BranchId", dto.BranchId),
                new SqlParameter("@PasswordHash", hashedPassword),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@LoginId", loginId)
            };

            return (await _context.Database
                .SqlQueryRaw<UserResponseDto>(sql, parameters)
                .ToListAsync()).First();
        }

        public async Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserRequestDto dto, int userId, int loginId)
        {
            var sql = @"EXEC usp_User
                        @Action = @Action,
                        @UserId = @UserId,
                        @Name = @Name,
                        @Role = @Role,
                        @BranchId = @BranchId,
                        @PerformedBy = @PerformedBy,
                        @LoginId = @LoginId";

            var parameters = new[]
            {
                new SqlParameter("@Action", "UPDATE"),
                new SqlParameter("@UserId", id),
                new SqlParameter("@Name", dto.Name ?? (object)DBNull.Value),
                new SqlParameter("@Role", dto.Role ?? (object)DBNull.Value),
                new SqlParameter("@BranchId", dto.BranchId ?? (object)DBNull.Value),
                new SqlParameter("@PerformedBy", userId),
                new SqlParameter("@LoginId", loginId)
            };

            return (await _context.Database
                .SqlQueryRaw<UserResponseDto>(sql, parameters)
                .ToListAsync()).First();
        }

        public async Task<UserResponseDto> UpdateUserStatusAsync(int id, ChangeUserStatusRequestDto dto, int userId, int loginId)
        {
            var sql = @"EXEC usp_User
                        @Action = @Action,
                        @UserId = @UserId,
                        @Status = @Status,
                        @IsLocked = @IsLocked,
                        @PerformedBy = @PerformedBy,
                        @LoginId = @LoginId";

            var parameters = new[]
            {
                new SqlParameter("@Action", "UPDATE_STATUS"),
                new SqlParameter("@UserId", id),
                new SqlParameter("@Status", dto.Status ?? (object)DBNull.Value),
                new SqlParameter("@IsLocked", dto.IsLocked ?? (object)DBNull.Value),
                new SqlParameter("@PerformedBy", userId),
                new SqlParameter("@LoginId", loginId)
            };

            return (await _context.Database
                .SqlQueryRaw<UserResponseDto>(sql, parameters)
                .ToListAsync()).First();
        }

        public async Task<(List<UserResponseDto>, PaginationDto)> GetUsersAsync(GetUsersRequestDto dto, int userId)
        {
            if (dto.UpdatedFrom > dto.UpdatedTo)
                throw new BusinessException("INVALID_DATE_RANGE",
                    "UpdatedFrom cannot be greater than UpdatedTo");

            var sql = @"EXEC usp_GetUsers 
                        @BranchId=@BranchId,
                        @Role=@Role,
                        @Status=@Status,
                        @IsLocked=@IsLocked,
                        @Search=@Search,
                        @CreatedFrom=@CreatedFrom,
                        @CreatedTo=@CreatedTo,
                        @UpdatedFrom=@UpdatedFrom,
                        @UpdatedTo=@UpdatedTo,
                        @SortBy=@SortBy,
                        @SortOrder=@SortOrder,
                        @Limit=@Limit,
                        @Offset=@Offset";

            var parameters = new[]
            {
                new SqlParameter("@Action", "GET_LIST"),
                new SqlParameter("@BranchId", dto.BranchId ?? (object)DBNull.Value),
                new SqlParameter("@Role", dto.Role ?? (object)DBNull.Value),
                new SqlParameter("@Status", dto.Status ?? (object)DBNull.Value),
                new SqlParameter("@IsLocked", dto.IsLocked ?? (object)DBNull.Value),
                new SqlParameter("@Search", dto.NameOrEmailSearch ?? (object)DBNull.Value),
                new SqlParameter("@CreatedFrom", dto.CreatedFrom ?? (object)DBNull.Value),
                new SqlParameter("@CreatedTo", dto.CreatedTo ?? (object)DBNull.Value),
                new SqlParameter("@UpdatedFrom", dto.UpdatedFrom ?? (object)DBNull.Value),
                new SqlParameter("@UpdatedTo", dto.UpdatedTo ?? (object)DBNull.Value),
                new SqlParameter("@SortBy", dto.SortBy ?? "Name"),
                new SqlParameter("@SortOrder", dto.SortOrder ?? "ASC"),
                new SqlParameter("@Limit", dto.Limit),
                new SqlParameter("@Offset", dto.Offset)
            };

            // 👇 Map to SP result DTO
            var spResult = await _context.Database
                .SqlQueryRaw<UserListSpResultDto>(sql, parameters)
                .ToListAsync();

            // 👇 Extract total count from first row
            int total = spResult.FirstOrDefault()?.TotalCount ?? 0;

            // 👇 Convert SP result to API response DTO
            var data = spResult.Select(x => new UserResponseDto
            {
                UserId = x.UserId,
                Name = x.Name,
                Email = x.Email,
                Role = x.Role,
                BranchId = x.BranchId,
                Status = x.Status,
                IsLocked = x.IsLocked,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();

            var pagination = new PaginationDto
            {
                Total = total,
                Limit = dto.Limit,
                Offset = dto.Offset
            };

            return (data, pagination);
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int userId)
        {
            var sql = @"EXEC usp_User
                        @Action = @Action,
                        @UserId = @UserId";

            var param = new[] {
                new SqlParameter("@Action", "GET_BY_ID"),
                new SqlParameter("@UserId", userId)
            };

            var result = await _context.Database
                .SqlQueryRaw<UserResponseDto>(sql, param)
                .ToListAsync();

            if (!result.Any())
                throw new BusinessException("USER_NOT_FOUND", "User not found.");

            return result.First();
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto, int userId, int loginId)
        {
            // 1. Call SP to get current hash
            var sqlGet = @"EXEC usp_User
                           @Action = @Action,
                           @UserId = @UserId";

            var userHash = (await _context.Database
                .SqlQueryRaw<string>(sqlGet,
                    new SqlParameter("@Action", "GET_PASSWORD_HASH"),
                    new SqlParameter("@UserId", userId))
                .ToListAsync())
                .FirstOrDefault();

            if (userHash == null)
                throw new BusinessException("USER_NOT_FOUND", "User not found.");

            bool isValid = BCrypt.Net.BCrypt.Verify(dto.OldPassword, userHash);

            if (!isValid)
                throw new BusinessException("OLD_PASSWORD_INVALID", "Your Old password is Wrong enter the correct one");

            var newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            var sqlUpdate = @"EXEC usp_User
                              @Action = @Action,
                              @UserId = @UserId,
                              @PasswordHash = @PasswordHash,
                              @LoginId = @LoginId";
            var parameters = new[]
            {
                new SqlParameter("@Action", "CHANGE_PASSWORD"),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@PasswordHash", newHash),
                new SqlParameter("@LoginId", loginId)
            };

            await _context.Database.ExecuteSqlRawAsync(sqlUpdate, parameters);

            return true;
        }

        public async Task<bool> FirstResetAsync(FirstPasswordResetRequestDto dto)
        {
            var sql = "EXEC USP_GetUserByEmail @Email";
            var parameters = new[]
            {
                new SqlParameter("@Email", dto.Email)
            };
            var users = await _context.Database
                .SqlQueryRaw<FindUserDto>(sql, parameters)
                .ToListAsync();

            var user = users.FirstOrDefault();

            if (user == null)
                throw new BusinessException("USER_NOT_FOUND", "User not found.");

            bool isValid = BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash);

            if (!isValid)
                throw new BusinessException("OLD_PASSWORD_INVALID", "Your Old password is Wrong enter the correct one");

            var newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            var sqlUpdate = "EXEC usp_ChangePassword @UserId,@PasswordHash";

            var parametersup = new[]
            {
                new SqlParameter("@UserId", user.UserId),
                new SqlParameter("@PasswordHash", newHash)
            };

            await _context.Database.ExecuteSqlRawAsync(sqlUpdate, parametersup);

            return true;
        }


    }
}