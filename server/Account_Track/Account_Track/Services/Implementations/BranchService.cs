using Account_Track.Data;
using Account_Track.Dtos.BranchDto;
using Account_Track.DTOs;
using Account_Track.DTOs.BranchDto;
using Account_Track.Services.Interfaces;
using Account_Track.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Account_Track.Services.Implementations
{
    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _context;

        public BranchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BranchResponseDto> CreateBranchAsync(CreateBranchRequestDto dto, int userId,int loginId)
        {
            var sql = @"EXEC usp_Branch
                        @Action = @Action,
                        @BranchName = @BranchName,
                        @IFSCCode = @IFSCCode,
                        @City = @City,
                        @State = @State,
                        @Country = @Country,
                        @Pincode = @Pincode,
                        @UserId = @UserId,
                        @LoginId = @LoginId";

            var parameters = new[]
            {
                new SqlParameter("@Action", "CREATE"),
                new SqlParameter("@BranchName", dto.BranchName),
                new SqlParameter("@IFSCCode", dto.IFSCCode),
                new SqlParameter("@City", dto.City),
                new SqlParameter("@State", dto.State),
                new SqlParameter("@Country", dto.Country),
                new SqlParameter("@Pincode", dto.Pincode),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@LoginId", loginId)
            };

            var result = await _context.Database
                .SqlQueryRaw<BranchResponseDto>(sql, parameters)
                .ToListAsync();

            if (!result.Any())
                throw new BusinessException("BRANCH_CREATION_FAILED", "Failed to create branch");

            return result.First();
        }

        public async Task<BranchResponseDto> UpdateBranchAsync(int branchId, UpdateBranchRequestDto dto, int userId,int loginId)
        {
            var sql = @"EXEC usp_Branch
                        @Action = @Action,
                        @BranchId = @BranchId,
                        @BranchName = @BranchName,
                        @IFSCCode = @IFSCCode,
                        @City = @City,
                        @State = @State,
                        @Country = @Country,
                        @Pincode = @Pincode,
                        @UserId = @UserId,
                        @LoginId = @LoginId";

            var parameters = new[]
            {
                new SqlParameter("@Action", "UPDATE"),
                new SqlParameter("@BranchId", branchId),
                new SqlParameter("@BranchName", (object?)dto.BranchName ?? DBNull.Value),
                new SqlParameter("@IFSCCode", (object?)dto.IFSCCode ?? DBNull.Value),
                new SqlParameter("@City", (object?)dto.City ?? DBNull.Value),
                new SqlParameter("@State", (object?)dto.State ?? DBNull.Value),
                new SqlParameter("@Country", (object?)dto.Country ?? DBNull.Value),
                new SqlParameter("@Pincode", (object?)dto.Pincode ?? DBNull.Value),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@LoginId", loginId)
            };

            var result = await _context.Database
                .SqlQueryRaw<BranchResponseDto>(sql, parameters)
                .ToListAsync();

            if (!result.Any())
                throw new KeyNotFoundException("BRANCH_NOT_FOUND");

            return result.First();
        }

        public async Task<(List<BranchListResponseDto>, PaginationDto)> GetBranchesAsync(GetBranchesRequestDto request)
        {

            var sql = @"EXEC usp_Branch
                        @Action = @Action,
                        @BranchId = @BranchId,
                        @BranchName = @BranchName,
                        @IFSCCode = @IFSCCode,
                        @City = @City,
                        @State = @State,
                        @Country = @Country,
                        @Pincode = @Pincode,
                        @SearchText = @SearchText,
                        @CreatedFrom = @CreatedFrom,
                        @CreatedTo = @CreatedTo,
                        @UpdatedFrom = @UpdatedFrom,
                        @UpdatedTo = @UpdatedTo,
                        @SortBy = @SortBy,
                        @SortOrder = @SortOrder,
                        @Limit = @Limit,
                        @Offset = @Offset";

            var parameters = new[]
            {
                new SqlParameter("@Action", "GET_LIST"),
                new SqlParameter("@BranchId", (object?)request.BranchId ?? DBNull.Value),
                new SqlParameter("@BranchName", (object?)request.BranchName ?? DBNull.Value),
                new SqlParameter("@IFSCCode", (object?)request.IFSCCode ?? DBNull.Value),
                new SqlParameter("@City", (object?)request.City ?? DBNull.Value),
                new SqlParameter("@State", (object?)request.State ?? DBNull.Value),
                new SqlParameter("@Country", (object?)request.Country ?? DBNull.Value),
                new SqlParameter("@Pincode", (object?)request.Pincode ?? DBNull.Value),
                new SqlParameter("@SearchText", (object?)request.SearchText ?? DBNull.Value),
                new SqlParameter("@CreatedFrom", (object?)request.CreatedFrom ?? DBNull.Value),
                new SqlParameter("@CreatedTo", (object?)request.CreatedTo ?? DBNull.Value),
                new SqlParameter("@UpdatedFrom", (object?)request.UpdatedFrom ?? DBNull.Value),
                new SqlParameter("@UpdatedTo", (object?)request.UpdatedTo ?? DBNull.Value),
                new SqlParameter("@SortBy", (object?)request.SortBy ?? DBNull.Value),
                new SqlParameter("@SortOrder", (object?)request.SortOrder ?? DBNull.Value),
                new SqlParameter("@Limit", request.Limit),
                new SqlParameter("@Offset", request.Offset)
            };

            var spResult = await _context.Database
                .SqlQueryRaw<BranchListSpResultDto>(sql, parameters)
                .ToListAsync();

            int total = spResult.FirstOrDefault()?.TotalCount ?? 0;

            var data = spResult.Select(x => new BranchListResponseDto
            {
                BranchId = x.BranchId,
                BranchName = x.BranchName,
                IFSCCode = x.IFSCCode,
                City = x.City,
                State = x.State,
                Country = x.Country,
                Pincode = x.Pincode,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();

            return (data, new PaginationDto { Total = total, Limit = request.Limit, Offset = request.Offset });
        }

        public async Task<BranchResponseDto> GetBranchByIdAsync(int branchId)
        {
            var sql = @"EXEC usp_Branch
                        @Action = @Action,
                        @BranchId = @BranchId";
   
            var result = await _context.Database
                .SqlQueryRaw<BranchResponseDto>(sql,
                    new SqlParameter("@Action", "GET_BY_ID"),
                    new SqlParameter("@BranchId", branchId))
                .ToListAsync();

            if (!result.Any())
                throw new KeyNotFoundException("BRANCH_NOT_FOUND");

            return result.First();
        }
    }
}