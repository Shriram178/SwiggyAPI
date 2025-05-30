using Foodiee.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiee.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserSyncService _userSyncService;

        public UserController(UserSyncService userSyncService)
        {
            _userSyncService = userSyncService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyUserProfile()
        {
            var user = await _userSyncService.SyncUserFromClaims(User);
            return Ok(user);
        }
    }
}
