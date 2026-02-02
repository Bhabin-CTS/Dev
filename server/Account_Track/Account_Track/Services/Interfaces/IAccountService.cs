
using Account_Track.DTOs;
using Account_Track.DTOs.AccountDto;


namespace Account_Track.Services.Interfaces
{
    public interface IAccountService
    {
        // POST vi/Create/Account
        Task<(bool Success, string? Error, AccountListItemDto? Data)> CreateAccountAsync(AccountCreateDto dto);

        // PUT vi/Account/{id}/edit
        Task<(bool Success, string? Error, AccountListItemDto? Data)> UpdateAccountAsync(int accountId, AccountUpdateDto dto);

        // GET vi/Account
        Task<(bool Success, string? Error, List<AccountListItemDto>? Data)> GetAllAccountsAsync();
        Task<(bool Success, string? Error, AccountListItemDto? Data)> GetAccountByIdAsync(int accountId);
    }
}
