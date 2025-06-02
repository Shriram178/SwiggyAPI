using System.Security.Claims;
using Foodiee.DTO;
using Foodiee.Repositories;
using Foodiee.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiee.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly KeycloakService _keycloakService;
        private readonly UserSyncService _userSyncService;

        public UserController(
            KeycloakService keycloakService,
            UserSyncService userSyncService)
        {
            _keycloakService = keycloakService;
            _userSyncService = userSyncService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            try
            {
                // 1. Create user in Keycloak (via admin API)
                await _keycloakService.RegisterNewUserAsync(dto);

                // 2. Sync that user into your own Postgres Db
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, dto.Username),
                    new("email", dto.Email),
                    new("address", dto.Address)
                };
                var identity = new ClaimsIdentity(claims);
                var userPrincipal = new ClaimsPrincipal(identity);

                await _userSyncService.SyncUserFromClaims(userPrincipal);

                return Ok(new { Message = "User registered successfully." });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Keycloak request failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var tokenResult = await _keycloakService.LoginAsync(dto);

                return Ok(tokenResult);
            }
            catch (HttpRequestException ex)
            {
                // If Keycloak rejects the username/password, bubble up 401:
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userSyncService.SyncUserFromClaims(User);
            return Ok(user);
        }
    }
}
