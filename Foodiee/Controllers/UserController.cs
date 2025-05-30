using Foodiee.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiee.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserSyncService _userSyncService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        UserSyncService userSyncService,
        ILogger<UserController> logger)
    {
        _userSyncService = userSyncService;
        _logger = logger;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyUserProfile()
    {
        try
        {
            var user = await _userSyncService.SyncUserFromClaims(User);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user profile");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("register/user")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDto dto)
    {
        try
        {
            var user = await _userSyncService.RegisterRegularUser(dto);
            return CreatedAtAction(nameof(GetMyUserProfile), user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User registration failed");
            return BadRequest("Registration failed");
        }
    }

    [HttpPost("register/restaurant")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterRestaurant([FromBody] RestaurantRegisterDto dto)
    {
        try
        {
            var user = await _userSyncService.RegisterRestaurant(dto);
            return CreatedAtAction(nameof(GetMyUserProfile), user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restaurant registration failed");
            return BadRequest("Restaurant registration failed");
        }
    }
}
