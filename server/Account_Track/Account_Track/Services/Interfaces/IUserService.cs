using AccountTrack.DTOs;

namespace Account_Track.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(int id, UserDto user);
    }
}

