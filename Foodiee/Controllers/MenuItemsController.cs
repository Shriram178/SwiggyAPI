using System.Linq.Dynamic.Core;
using Foodiee.DTO;
using Foodiee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodiee.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MenuItemsController : ControllerBase
    {
        private readonly FoodieeDbContext _context;

        public MenuItemsController(FoodieeDbContext context)
        {
            _context = context;
        }

        [HttpPost(Name = "AddMenuItem")]
        public async Task<ActionResult<RestDTO<MenuItemDTO>>> AddMenuItem([FromBody] CreateMenuItemDTO dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(dto.RestaurantId);
            if (restaurant == null)
                return NotFound("Restaurant not found");

            var menuItem = new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description ?? "",
                Price = dto.Price,
                RestaurantId = dto.RestaurantId,
                IsAvailable = true
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, new RestDTO<MenuItemDTO>
            {
                Data = new MenuItemDTO
                {
                    Id = menuItem.Id,
                    Name = menuItem.Name,
                    Price = menuItem.Price
                }
            });
        }

        [HttpGet("restaurant/{restaurantId}", Name = "GetMenuForRestaurant")]
        public async Task<ActionResult<RestDTO<MenuItemDTO[]>>> GetMenuForRestaurant(
     Guid restaurantId,
     [FromQuery] RequestDTO input)
        {
            var query = _context.MenuItems
                .Where(mi => mi.RestaurantId == restaurantId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(input.FilterQuery))
                query = query.Where(mi => mi.Name.Contains(input.FilterQuery));

            var recordCount = await query.CountAsync();

            query = query
                .OrderBy($"{input.SortColumn} {input.SortOrder}")
                .Skip(input.PageIndex * input.PageSize)
                .Take(input.PageSize);

            var dtos = await query
                .Select(mi => new MenuItemDTO
                {
                    Id = mi.Id,
                    Name = mi.Name,
                    Price = mi.Price
                })
                .ToArrayAsync();

            var selfHref = Url.Link(
                routeName: "GetMenuForRestaurant",
                values: new
                {
                    restaurantId,
                    input.PageIndex,
                    input.PageSize,
                    input.FilterQuery,
                    input.SortColumn,
                    input.SortOrder
                }
            )!;

            var response = new RestDTO<MenuItemDTO[]>
            {
                Data = dtos,
                RecordCount = recordCount,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                Links = new List<LinkDTO> {
            new LinkDTO(selfHref, "self", "GET")
        }
            };

            return Ok(response);
        }


        [HttpGet("{id}", Name = "GetMenuItem")]
        public async Task<ActionResult<RestDTO<MenuItemDTO>>> GetMenuItem(Guid id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
                return NotFound();

            return Ok(new RestDTO<MenuItemDTO>
            {
                Data = new MenuItemDTO
                {
                    Id = menuItem.Id,
                    Name = menuItem.Name,
                    Price = menuItem.Price
                }
            });
        }
    }

}
