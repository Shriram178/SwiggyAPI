using System.Linq.Dynamic.Core;
using Foodiee.DTO;
using Foodiee.Models;
using Foodiee.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodiee.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MenuItemsController : ControllerBase
    {
        private readonly IMenuItemRepository _repository;

        public MenuItemsController(
            IMenuItemRepository repository)
        {
            _repository = repository;
        }

        [HttpPost(Name = "AddMenuItem")]
        public async Task<ActionResult<RestDTO<MenuItemDTO>>> AddMenuItemAsync(
            [FromBody] CreateMenuItemDTO dto)
        {
            var menuItem = new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description ?? "",
                Price = dto.Price,
                RestaurantId = dto.RestaurantId,
                IsAvailable = true
            };


            var addedToMenu = await _repository.AddAsync(menuItem);
            if (addedToMenu == null)
                return NotFound("Restaurant not found");

            return CreatedAtAction(nameof(GetMenuItemAsync), new { id = addedToMenu.Id }, new RestDTO<MenuItemDTO>
            {
                Data = new MenuItemDTO
                {
                    Id = addedToMenu.Id,
                    Name = addedToMenu.Name,
                    Price = addedToMenu.Price,
                    IsAvailable = addedToMenu.IsAvailable,
                    Description = addedToMenu.Description
                }
            });
        }

        [HttpGet("restaurant/{restaurantId}", Name = "GetMenuForRestaurant")]
        public async Task<ActionResult<RestDTO<MenuItemDTO[]>>> GetMenuForRestaurantAsync(
            Guid restaurantId, [FromQuery] RequestDTO input)
        {
            var (query, recordCount) = await _repository.GetPagedAsync(restaurantId, input);

            var dtos = query
                .Select(mi => new MenuItemDTO
                {
                    Id = mi.Id,
                    Name = mi.Name,
                    Price = mi.Price,
                    Description = mi.Description,
                    IsAvailable = mi.IsAvailable
                }).ToArray();

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

        [HttpPut("Update", Name = "UpdateMenuItem")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<MenuItemDTO?>>> UpdateMenuItemAsync(
            [FromBody] MenuItemDTO model)
        {

            var toUpdate = new MenuItem
            {
                Id = model.Id,
                Name = model.Name,
                Price = model.Price,
                IsAvailable = model.IsAvailable,
                Description = model.Description
            };

            var updatedItem = await _repository.UpdateAsync(toUpdate);

            if (updatedItem == null)
                return NotFound("Menu Item not found");

            var dto = new MenuItemDTO
            {
                Id = updatedItem.Id,
                Name = updatedItem.Name,
                Price = updatedItem.Price,
                Description = updatedItem.Description,
                IsAvailable = updatedItem.IsAvailable
            };

            var links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(
                        null,
                        nameof(GetMenuItemAsync),
                        new { id = updatedItem.Id },
                        Request.Scheme)!,
                    "self",
                    "POST"),
            };

            return Ok(new RestDTO<MenuItemDTO?>
            {
                Data = dto,
                Links = links
            });
        }


        [HttpGet("{id}", Name = "GetMenuItem")]
        public async Task<ActionResult<RestDTO<MenuItemDTO>>> GetMenuItemAsync(Guid id)
        {
            var menuItem = await _repository.GetByIdAsync(id);
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

        [HttpDelete("{id}", Name = "DeleteMenuItem")]
        public async Task<ActionResult<RestDTO<MenuItemDTO>>> DeleteMenuItemAsync(
            Guid id)
        {
            var item = await _repository.DeleteAsync(id);
            if (item == null)
                return NotFound("Menu Item not found");

            MenuItemDTO dto = new MenuItemDTO
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Description = item.Description,
                IsAvailable = item.IsAvailable
            };

            var response = new RestDTO<MenuItemDTO>
            {
                Data = dto,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            nameof(GetMenuItemAsync),
                            new { id = item.Id },
                            Request.Scheme)!,
                        "deleted",
                        "DELETE")
                }
            };

            return Ok(response);

        }
    }
}
