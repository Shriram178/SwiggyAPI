using Foodiee.DTO;
using Foodiee.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodiee.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet("orders/{orderId}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound(new RestDTO<OrderDTO> { Data = null });

            var orderDto = new OrderDTO
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                DeliveryAddress = order.DeliveryAddress,
                DeliveryAgentName = order.DeliveryAgent?.Name ?? "Not Assigned",
                RestaurantName = order.Restaurant?.Name ?? "Unknown",
                OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    MenuItemId = oi.MenuItemId,
                    ItemName = oi.MenuItem?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    Price = oi.MenuItem?.Price ?? 0
                }).ToList()
            };

            return Ok(orderDto);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<RestDTO<List<OrderDTO>>>> GetOrders(Guid userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            if (orders == null || !orders.Any())
                return NotFound(new RestDTO<List<OrderDTO>> { Data = null });

            var dtoList = orders.Select(order => new OrderDTO
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                DeliveryAddress = order.DeliveryAddress,
                DeliveryAgentName = order.DeliveryAgent?.Name,
                RestaurantName = order.Restaurant?.Name ?? string.Empty,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    MenuItemId = oi.MenuItemId,
                    ItemName = oi.MenuItem?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    Price = oi.MenuItem?.Price ?? 0
                }).ToList()
            }).ToList();

            return Ok(new RestDTO<List<OrderDTO>>
            {
                Data = dtoList
            });
        }


        // POST: api/order/place?userId=...
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromQuery] Guid userId)
        {
            try
            {
                var order = await _orderRepository.PlaceOrderFromCartAsync(userId);

                var orderDto = new OrderDTO
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status.ToString(),
                    DeliveryAddress = order.DeliveryAddress,
                    DeliveryAgentName = order.DeliveryAgent?.Name,
                    RestaurantName = order.Restaurant.Name,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                    {
                        MenuItemId = oi.MenuItemId,
                        ItemName = oi.MenuItem.Name,
                        Quantity = oi.Quantity,
                        Price = oi.MenuItem.Price
                    }).ToList(),
                };

                return Ok(new { Message = "Order placed successfully.", orderDto });
            }
            catch (Exception ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { Message = "Failed to place order.", Detail = detail });
            }
        }
    }
}
