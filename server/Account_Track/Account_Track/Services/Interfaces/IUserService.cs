using Account_Track.Dtos.UserDto;
using Account_Track.DTOs;
using Account_Track.DTOs.UsersDto;

namespace Account_Track.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto dto, int userId,int loginId);

        Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserRequestDto dto, int userId, int loginId);

        Task<UserResponseDto> UpdateUserStatusAsync(int id, ChangeUserStatusRequestDto dto, int userId,int loginId);
        Task<(List<UserResponseDto>, PaginationDto)> GetUsersAsync(GetUsersRequestDto dto, int userId);

        Task<UserResponseDto> GetUserByIdAsync(int userId);

        Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto, int userId,int loginId);
        Task<bool> FirstResetAsync(FirstPasswordResetRequestDto dto);
    }
}
