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

        // POST: api/order/place?userId=...
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromQuery] Guid userId)
        {
            try
            {
                var orderId = await _orderRepository.PlaceOrderFromCartAsync(userId);
                return Ok(new { Message = "Order placed successfully.", OrderId = orderId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
