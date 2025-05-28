using Foodiee.Models;

namespace Foodiee.Repositories
{
    public interface IOrderRepository
    {
        Task<Guid> PlaceOrderFromCartAsync(Guid userId);
        Task<List<Order>> GetOrdersByUserIdAsync(Guid userId);
        Task<Order?> GetOrderByIdAsync(Guid orderId);
    }
}
