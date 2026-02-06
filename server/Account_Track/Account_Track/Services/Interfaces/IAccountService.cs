using System.Collections.Generic;
using System.Threading.Tasks;
using Account_Track.DTOs;
using Account_Track.DTOs.AccountDto;

namespace Account_Track.Services.Interfaces
{
    public interface IAccountService
    {
        Task<CreateAccountResponseDto> CreateAccountAsync(CreateAccountRequestDto dto, int userId);

        Task<(List<AccountListResponseDto> Items, PaginationDto Pagination)> GetAccountsAsync(
            GetAccountsRequestDto request, int userId);

        Task<AccountDetailResponseDto> GetAccountByIdAsync(int accountId, int userId);

        Task<AccountDetailResponseDto> UpdateAccountAsync(int accountId, UpdateAccountRequestDto dto, int userId);
    }
}