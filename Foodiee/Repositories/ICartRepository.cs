using Foodiee.Models;

namespace Foodiee.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByUserIdAsync(Guid userId);
        Task AddItemToCartAsync(Guid userId, Guid menuItemId, int quantity);
        Task RemoveItemFromCartAsync(Guid userId, Guid cartItemId);
        Task ClearCartAsync(Guid userId);

        Task SaveChangesAsync();
    }
}
