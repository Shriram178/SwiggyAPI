using Foodiee.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodiee.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly FoodieeDbContext _context;

        public CartRepository(FoodieeDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.MenuItem)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task AddItemToCartAsync(Guid userId, Guid menuItemId, int quantity)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CartItems = new List<CartItem>()
                };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.MenuItemId == menuItemId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    Cart = cart,
                    MenuItemId = menuItemId,
                    Quantity = quantity
                };

                await _context.CartItems.AddAsync(newItem);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemFromCartAsync(Guid userId, Guid cartItemId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            var item = cart?.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
            }
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
