using System.Security.Claims;
using Foodiee.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodiee.Repositories
{
    public class UserSyncService
    {
        private readonly FoodieeDbContext _context;

        public UserSyncService(FoodieeDbContext context)
        {
            _context = context;
        }

        public async Task<User> SyncUserFromClaims(ClaimsPrincipal userClaims)
        {
            var username = userClaims.Identity?.Name;
            var email = userClaims.FindFirst("email")?.Value;
            var address = userClaims.FindFirst("address")?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username!,
                    Email = email,
                    Address = address,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return user;
        }
    }

}
