using Account_Track.DTOs;
using Account_Track.Dtos.BranchDto;

namespace Account_Track.Services.Interfaces
{
    public interface IBranchService
    {
        Task<BranchResponse> CreateBranchAsync(CreateBranchRequest dto, int userId);
        Task<BranchResponse> UpdateBranchAsync(int branchId, UpdateBranchRequest dto, int userId);

        Task<(List<BranchResponse> Data, PaginationDto Pagination)> GetBranchesAsync(
            string? searchTerm, string? city, string? state, string? sortBy, string? sortOrder,
            int limit, int offset, int userId);

        Task<BranchResponse> GetBranchByIdAsync(int branchId, int userId);
    }
}