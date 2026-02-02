using Account_Track.Services.Interfaces;
using AccountTrack.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Account_Track.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET api/users/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // PUT api/users/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto user)
        {
            if (user == null)
                return BadRequest(new { error = "User data is required." });

            if (user.UserID != 0 && user.UserID != id)
                return BadRequest(new { error = "Route id and body userID must match." });

            var updated = await _userService.UpdateUserAsync(id, user);

            if (!updated)
                return NotFound(new { error = $"User with ID {id} not found or update failed." });

            return Ok(new { message = "User updated successfully." });
        }

    }
}
