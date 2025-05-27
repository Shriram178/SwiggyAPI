using System.Linq.Dynamic.Core;
using Foodiee.DTO;
using Foodiee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodiee.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly FoodieeDbContext _context;

        public RestaurantsController(FoodieeDbContext context)
        {
            _context = context;
        }

        // POST: api/Restaurants
        [HttpPost(Name = "CreateRestaurant")]
        public async Task<ActionResult<RestDTO<RestaurantDTO>>> CreateRestaurant(CreateRestaurantDTO dto)
        {
            var restaurant = new Restaurant
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                City = dto.City,
                Description = dto.Description,


            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var response = new RestDTO<RestaurantDTO>
            {
                Data = new RestaurantDTO
                {
                    Id = restaurant.Id,
                    Name = restaurant.Name,
                    Address = restaurant.Address,
                    PhoneNumber = restaurant.PhoneNumber
                }
            };

            return CreatedAtAction(nameof(GetRestaurant), new { id = restaurant.Id }, response);
        }

        [HttpGet(Name = "GetRestaurants")]
        public async Task<ActionResult<RestDTO<RestaurantDTO[]>>> GetRestaurants([FromQuery] RequestDTO input)
        {


            var query = _context.Restaurants.AsQueryable();
            if (!string.IsNullOrEmpty(input.FilterQuery))
                query = query.Where(b => b.Name.Contains(input.FilterQuery));
            var recordCount = await query.CountAsync();
            query = query
                .OrderBy($"{input.SortColumn} {input.SortOrder}")
                .Skip(input.PageIndex * input.PageSize)
                .Take(input.PageSize);

            var dtos = await query
                .Select(r => new RestaurantDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Address = r.Address,
                    PhoneNumber = r.PhoneNumber,
                    Description = r.Description
                })
                .ToArrayAsync();

            var links = new List<LinkDTO> {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "Restaurants",
                            new { input.PageIndex, input.PageSize },
                            Request.Scheme)!,
                        "self",
                        "GET"),
                    };

            var response = new RestDTO<RestaurantDTO[]>
            {
                Data = dtos,
                Links = links,
                RecordCount = recordCount,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize
            };



            return Ok(response);
        }

        // GET: api/Restaurants/{id}
        [HttpGet("{id}", Name = "GetRestaurant")]
        public async Task<ActionResult<RestDTO<RestaurantDTO>>> GetRestaurant(Guid id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant == null)
                return NotFound();

            var response = new RestDTO<RestaurantDTO>
            {
                Data = new RestaurantDTO
                {
                    Id = restaurant.Id,
                    Name = restaurant.Name,
                    Address = restaurant.Address,
                    PhoneNumber = restaurant.PhoneNumber
                }
            };

            return Ok(response);
        }

        [HttpPost("Update", Name = "UpdateRestaurant")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<RestaurantDTO?>> UpdateRestaurant([FromBody] RestaurantDTO model)
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == model.Id);

            if (restaurant != null)
            {
                if (!string.IsNullOrEmpty(model.Name))
                    restaurant.Name = model.Name;
                if (!string.IsNullOrEmpty(model.Address))
                    restaurant.Address = model.Address;
                if (!string.IsNullOrEmpty(model.Description))
                    restaurant.Description = model.Description;
                if (!string.IsNullOrEmpty(model.PhoneNumber))
                    restaurant.PhoneNumber = model.PhoneNumber;

                _context.Restaurants.Update(restaurant);
                await _context.SaveChangesAsync();
            }

            var dto = restaurant != null ? new RestaurantDTO
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
                PhoneNumber = restaurant.PhoneNumber,
                Description = restaurant.Description
            } : null;

            return new RestDTO<RestaurantDTO?>()
            {
                Data = dto,
                Links = new List<LinkDTO>
        {
            new LinkDTO(
                Url.Action(
                    null,
                    "Restaurants",
                    model,
                    Request.Scheme)!,
                "self",
                "POST"),
        }
            };
        }

        [HttpDelete("{id}", Name = "DeleteRestaurant")]
        public async Task<ActionResult<RestDTO<RestaurantDTO>>> DeleteRestaurant(Guid id)
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return NotFound();

            // Map to DTO before deletion
            var dto = new RestaurantDTO
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
                PhoneNumber = restaurant.PhoneNumber,
                Description = restaurant.Description
            };

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();

            var response = new RestDTO<RestaurantDTO>
            {
                Data = dto,
                Links = new List<LinkDTO>
        {
            new LinkDTO(
                Url.Action("GetRestaurant", "Restaurants", new { id = dto.Id }, Request.Scheme)!,
                "deleted",
                "DELETE"
            )
        }
            };

            return Ok(response);
        }

    }
}
