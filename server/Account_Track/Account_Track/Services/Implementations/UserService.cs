using Account_Track.Data;
using Account_Track.Utils.Enum;
using Account_Track.Services.Interfaces;
using AccountTrack.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace AccountTrack.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public object UserDto => throw new NotImplementedException();

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                var pUserId = new SqlParameter("@UserID", SqlDbType.Int) { Value = userId };

                var users = await _context.Users
                    .FromSqlRaw("EXEC dbo.usp_GetUserById @UserID", pUserId)
                    .AsNoTracking()
                    .ToListAsync();

                var user = users.FirstOrDefault();
                if (user == null) return null;


                var branchName = await _context.Branches
                            .Where(b => b.BranchId == user.BranchId)
                            .Select(b => b.BranchName)
                            .FirstOrDefaultAsync();


                return new UserDto
                {
                    UserID = user.UserId,
                    Name = user.Name,
                    Role = user.Role.ToString(),
                    Email = user.Email,
                    Branch = branchName ?? string.Empty,
                    PasswordHash = user.PasswordHash,
                    Status = user.Status.ToString()
                };
            }
            catch (Exception ex)
            {
                return new UserDto { ErrorMessage = $"GetUserById failed: {ex.Message}" };
            }
        }

        public async Task<bool> UpdateUserAsync(int userId, UserDto userDto)
        {
            try
            {
                var existing = await _context.Users.FindAsync(userId);
                if (existing == null) return false;

                // Only update provided fields
                if (!string.IsNullOrEmpty(userDto.Name)) existing.Name = userDto.Name;
                if (!string.IsNullOrEmpty(userDto.Email)) existing.Email = userDto.Email;
                if (!string.IsNullOrEmpty(userDto.PasswordHash)) existing.PasswordHash = userDto.PasswordHash;

                if (!string.IsNullOrEmpty(userDto.Role) &&
                    Enum.TryParse<UserRole>(userDto.Role, true, out var parsedRole))
                {
                    existing.Role = parsedRole;
                }

                if (!string.IsNullOrEmpty(userDto.Status) &&
                    Enum.TryParse<UserStatus>(userDto.Status, true, out var parsedStatus))
                {
                    existing.Status = parsedStatus;
                }


                if (userDto.BranchId.HasValue)
                {
                    var exists = await _context.Branches.AnyAsync(b => b.BranchId == userDto.BranchId.Value);
                    if (!exists) return false; // or throw/return error "Invalid branch."
                    existing.BranchId = userDto.BranchId.Value;
                }

                else if (!string.IsNullOrWhiteSpace(userDto.Branch))
                {
                    var branch = await _context.Branches
                        .SingleOrDefaultAsync(b => b.BranchName == userDto.Branch);
                    if (branch == null)
                    {
                        // Option A: return failure
                        return false; // or return false with an error message
                                      // Option B: create it automatically (only if your business allows)
                                      // branch = new t_Branch { Name = userDto.Branch };
                                      // _context.Branches.Add(branch);
                                      // await _context.SaveChangesAsync();
                    }

                    existing.BranchId = branch!.BranchId;
                }



                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateUser failed: {ex.Message}");
                return false;
            }
        }
    }
}
