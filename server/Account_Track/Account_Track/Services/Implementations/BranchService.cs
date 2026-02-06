using Account_Track.Data;
using Account_Track.DTOs;
using Account_Track.Dtos.BranchDto;
using Account_Track.Model;
using Account_Track.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Account_Track.Services.Implementations
{
    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _db;

        public BranchService(ApplicationDbContext db)
        {
            _db = db;
        }

        private static string? ToIso(DateTime? dt) => dt?.ToUniversalTime().ToString("o");

        private static BranchResponse Map(t_Branch b) => new()
        {
            BranchId = b.BranchId,
            BranchName = b.BranchName,
            IFSCCode = b.IFSCCode,
            City = b.City,
            State = b.State,
            Country = b.Country,
            Pincode = b.Pincode,
            UpdatedAt = ToIso(b.UpdatedAt)
        };

        public async Task<BranchResponse> CreateBranchAsync(CreateBranchRequest dto, int userId)
        {
            // SP must return exactly 1 row of t_Branch
            var list = await _db.Branches
                .FromSqlInterpolated($@"
                    EXEC dbo.usp_Branch_Create
                        @BranchName={dto.BranchName},
                        @IFSCCode={dto.IFSCCode},
                        @City={dto.City},
                        @State={dto.State},
                        @Country={dto.Country},
                        @Pincode={dto.Pincode},
                        @PerformedByUserId={userId},
                        @LoginId={userId}
                ")
                .AsNoTracking()
                .ToListAsync();

            if (list.Count != 1)
                throw new ArgumentException("Create SP did not return a single row");

            return Map(list[0]);
        }

        public async Task<BranchResponse> UpdateBranchAsync(int branchId, UpdateBranchRequest dto, int userId)
        {
            var list = await _db.Branches
                .FromSqlInterpolated($@"
                    EXEC dbo.usp_Branch_Update
                        @BranchId={branchId},
                        @BranchName={dto.BranchName},
                        @IFSCCode={dto.IFSCCode},
                        @City={dto.City},
                        @State={dto.State},
                        @Country={dto.Country},
                        @Pincode={dto.Pincode},
                        @PerformedByUserId={userId},
                        @LoginId={userId}
                ")
                .AsNoTracking()
                .ToListAsync();

            if (list.Count == 0)
                throw new KeyNotFoundException("BRANCH_NOT_FOUND"); // in case SP selected nothing
            if (list.Count != 1)
                throw new ArgumentException("Update SP did not return a single row");

            return Map(list[0]);
        }

        public async Task<(List<BranchResponse> Data, PaginationDto Pagination)> GetBranchesAsync(
            string? searchTerm, string? city, string? state, string? sortBy, string? sortOrder,
            int limit, int offset, int userId)
        {
            if (limit <= 0 || limit > 100 || offset < 0)
                throw new ArgumentException("Invalid pagination parameters");

            var q = _db.Branches.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.Trim();
                q = q.Where(b => b.BranchName.Contains(s) || b.IFSCCode.Contains(s) || b.City.Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(city)) q = q.Where(b => b.City == city);
            if (!string.IsNullOrWhiteSpace(state)) q = q.Where(b => b.State == state);

            bool desc = string.Equals(sortOrder, "DESC", StringComparison.OrdinalIgnoreCase);
            q = (sortBy?.ToLowerInvariant()) switch
            {
                "name" => desc ? q.OrderByDescending(b => b.BranchName) : q.OrderBy(b => b.BranchName),
                "ifsc" => desc ? q.OrderByDescending(b => b.IFSCCode) : q.OrderBy(b => b.IFSCCode),
                "city" => desc ? q.OrderByDescending(b => b.City) : q.OrderBy(b => b.City),
                "createdat" => desc ? q.OrderByDescending(b => b.CreatedAt) : q.OrderBy(b => b.CreatedAt),
                _ => q.OrderBy(b => b.BranchName)
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

        public async Task<BranchResponse> GetBranchByIdAsync(int branchId, int userId)
        {
            var entity = await _db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.BranchId == branchId);
            if (entity == null)
                throw new KeyNotFoundException("BRANCH_NOT_FOUND");

            return Map(entity);
        }
    }
}