using Foodiee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodiee.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly FoodieeDbContext _context;

        private readonly ILogger<SeedController> _logger;
        public SeedController(FoodieeDbContext context, ILogger<SeedController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPut]
        public async Task<IActionResult> Seed()
        {
            if (_context.Users.Any() || _context.Restaurants.Any() || _context.MenuItems.Any())
                return BadRequest("Database has already been seeded.");

            var now = DateTime.UtcNow;

            var user = new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Username = "testuser",
                Email = "test@example.com",
                Password = "hashedpassword123", // Replace with real hashed value
                Address = "123 Main Street, Testville",
                CreatedAt = now
            };

            var restaurant = new Restaurant
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Tasty Bites",
                Address = "456 Food Avenue",
                City = "SampleCity",
                Description = "Delicious food served fast",
                PhoneNumber = "987-654-3210",
                OwnerId = user.Id,
                IsOpen = true
            };

            var menuItem1 = new MenuItem
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Cheeseburger",
                Description = "Juicy grilled burger with cheese",
                Price = 149.99m,
                IsAvailable = true,
                RestaurantId = restaurant.Id
            };

            var menuItem2 = new MenuItem
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Fries",
                Description = "Crispy golden fries",
                Price = 49.99m,
                IsAvailable = true,
                RestaurantId = restaurant.Id
            };

            _context.Users.Add(user);
            _context.Restaurants.Add(restaurant);
            _context.MenuItems.AddRange(menuItem1, menuItem2);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Seeding completed successfully.",
                Users = await _context.Users.CountAsync(),
                Restaurants = await _context.Restaurants.CountAsync(),
                MenuItems = await _context.MenuItems.CountAsync()
            });
        }
    }
}
