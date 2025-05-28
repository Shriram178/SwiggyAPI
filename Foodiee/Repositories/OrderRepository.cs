using System.Linq.Dynamic.Core;
using Foodiee.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodiee.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FoodieeDbContext _context;

        public OrderRepository(FoodieeDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.Orders.FindAsync(orderId);
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .ToListAsync();
        }

        public async Task<Guid> PlaceOrderFromCartAsync(Guid userId)
        {
            var cart = await _context.Carts
        .Include(c => c.CartItems)
            .ThenInclude(ci => ci.MenuItem)
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                throw new Exception("Cart is empty or not found.");

            decimal totalAmount = cart.CartItems
                .Sum(item => item.MenuItem.Price * item.Quantity);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RestaurantId = cart.CartItems.First().MenuItem.RestaurantId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                DeliveryAddress = cart.User.Address,
                DeliveryAgentId = null,
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    OrderId = order.Id
                });
            }

            await _context.Orders.AddAsync(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return order.Id;
        }
    }
}
