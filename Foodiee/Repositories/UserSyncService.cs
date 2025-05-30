using System.Security.Claims;
using Foodiee.DTO;
using Foodiee.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodiee.Repositories;

public class UserSyncService
{
    private readonly FoodieeDbContext _context;
    private readonly ILogger<UserSyncService> _logger;

    public UserSyncService(FoodieeDbContext context, ILogger<UserSyncService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User> SyncUserFromClaims(ClaimsPrincipal userClaims)
    {
        var keycloakId = userClaims.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(keycloakId))
        {
            throw new ArgumentException("Invalid user claims - missing sub");
        }

        var user = await _context.Users
            .Include(u => u.Restaurant)
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        if (user == null)
        {
            user = new User
            {
                KeycloakId = keycloakId,
                Username = userClaims.FindFirst("preferred_username")?.Value,
                Email = userClaims.FindFirst("email")?.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return user;
    }

    public async Task<User> RegisterRegularUser(UserRegisterDto dto)
    {
        var user = new User
        {
            KeycloakId = dto.KeycloakId,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow,
            RestaurantId = null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> RegisterRestaurant(RestaurantRegisterDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var restaurant = new Restaurant
            {
                Id = Guid.NewGuid(),
                Name = dto.RestaurantName,
                Address = dto.RestaurantAddress,
                // Initialize other restaurant properties
            };

            var user = new User
            {
                Id = restaurant.Id, // Shared ID
                KeycloakId = dto.KeycloakId,
                Email = dto.Email,
                Restaurant = restaurant,
                CreatedAt = DateTime.UtcNow
            };

            _context.Restaurants.Add(restaurant);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return user;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to register restaurant");
            throw;
        }
    }
}