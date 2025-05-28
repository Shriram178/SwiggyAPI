using Foodiee.DTO;
using Foodiee.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodiee.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepo;

        public CartController(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        // GET: api/Cart/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<RestDTO<CartDTO>>> GetCart(Guid userId)
        {
            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null)
                return NotFound(new RestDTO<CartDTO> { Data = null });

            var dto = new CartDTO
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemDTO
                {
                    Id = ci.Id,
                    MenuItemId = ci.MenuItemId,
                    Quantity = ci.Quantity,
                    ItemName = ci.MenuItem.Name,
                    Price = ci.MenuItem.Price
                }).ToList()
            };

            return Ok(new RestDTO<CartDTO> { Data = dto });
        }

        // POST: api/Cart/{userId}/items
        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddToCart(Guid userId, [FromBody] AddToCartDTO addDto)
        {
            await _cartRepo.AddItemToCartAsync(userId, addDto.MenuItemId, addDto.Quantity);
            await _cartRepo.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Cart/{userId}/items/{itemId}
        [HttpDelete("{userId}/items/{itemId}")]
        public async Task<IActionResult> RemoveItem(Guid userId, Guid itemId)
        {
            await _cartRepo.RemoveItemFromCartAsync(userId, itemId);
            await _cartRepo.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Cart/{userId}
        [HttpDelete("{userId}")]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _cartRepo.ClearCartAsync(userId);
            await _cartRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}
