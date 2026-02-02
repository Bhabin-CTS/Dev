using Microsoft.AspNetCore.Mvc;
using Account_Track.Services.Interfaces;
using Account_Track.DTOs;
using Account_Track.DTOs.AccountDto;

namespace Account_Track.Controllers
{
    // Intentionally NOT using [ApiController] to return custom validation strings.
    [ApiController]
    [Route("v1/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        // ---------------------------------------------------------
        // POST vi/Create/Account
        // Create a new account
        // ---------------------------------------------------------
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] AccountCreateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Request body is required.");

                if (!ModelState.IsValid)
                    return BadRequest(AggregateModelErrors());

                var result = await _accountService.CreateAccountAsync(dto);

                if (!result.Success)
                    return BadRequest(result.Error); // plain string as per team lead

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Create Account endpoint.");
                return StatusCode(500, "Unexpected error while creating account.");
            }
        }

        // ---------------------------------------------------------
        // PUT vi/Account/{id}/edit
        // Update an account with optimistic concurrency support
        // ---------------------------------------------------------
        [HttpPut("{id:int}/edit")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] AccountUpdateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Request body is required.");

                if (!ModelState.IsValid)
                    return BadRequest(AggregateModelErrors());

                var result = await _accountService.UpdateAccountAsync(id, dto);

                if (!result.Success)
                    return BadRequest(result.Error);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Edit Account endpoint for AccountID {AccountID}", id);
                return StatusCode(500, "Unexpected error while updating account.");
            }
        }

        // ---------------------------------------------------------
        // GET vi/Account
        // List all accounts
        // ---------------------------------------------------------
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _accountService.GetAllAccountsAsync();

                if (!result.Success)
                    return BadRequest(result.Error);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAll Accounts endpoint.");
                return StatusCode(500, "Unexpected error while fetching accounts.");
            }
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var (ok, err, data) = await _accountService.GetAccountByIdAsync(id);
            if (!ok)
            {
                if (err != null && err.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(err);
                return BadRequest(err ?? "Failed to fetch account.");
            }
            return Ok(data);
        }



        // ---------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------
        private string AggregateModelErrors()
        {
            // Build a single string of all validation errors separated by new lines
            return string.Join("\n",
                ModelState.Values
                          .SelectMany(v => v.Errors)
                          .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                                        ? "Invalid input."
                                        : e.ErrorMessage));
        }
    }
}
