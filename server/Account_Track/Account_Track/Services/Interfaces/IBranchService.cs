using Account_Track.Dtos.BranchDto;
using Account_Track.DTOs;
using Account_Track.DTOs.BranchDto;

namespace Account_Track.Services.Interfaces
{
    public interface IBranchService
    {
        Task<BranchResponseDto> CreateBranchAsync(CreateBranchRequestDto dto, int userId);

        Task<BranchResponseDto> UpdateBranchAsync(int branchId, UpdateBranchRequestDto dto, int userId);

        Task<(List<BranchListResponseDto>, PaginationDto)> GetBranchesAsync(GetBranchesRequestDto request);

        Task<BranchResponseDto> GetBranchByIdAsync(int branchId);
    }
}