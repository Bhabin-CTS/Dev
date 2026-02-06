// File: Account_Track/Services/Interfaces/IUserService.cs
using Account_Track.DTOs;
using Account_Track.Dtos.UserDto;

namespace Account_Track.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> CreateUserAsync(CreateUserRequest dto, int userId);
        Task<UserResponse> UpdateUserAsync(int targetUserId, UpdateUserRequest dto, int userId);
        Task<UserResponse> UpdateUserStatusAsync(int targetUserId, ChangeUserStatusRequest dto, int userId);

        Task<(List<UserResponse> Data, PaginationDto Pagination)> GetUsersAsync(
            int? branchId, string? role, string? status, string? searchTerm,
            string? sortBy, string? sortOrder, int limit, int offset, int userId);

        Task<UserResponse> GetUserByIdAsync(int targetUserId, int userId);
        Task<UserResponse?> GetUserByEmailAsync(string email, int userId);
    }
}
