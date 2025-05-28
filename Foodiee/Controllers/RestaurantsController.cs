using Foodiee.DTO;
using Foodiee.Models;
using Foodiee.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodiee.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {

        private readonly IRestaurantRepository _repository;

        public RestaurantsController(IRestaurantRepository repository)
        {
            _repository = repository;
        }

        // POST: api/Restaurants
        [HttpPost(Name = "CreateRestaurant")]
        public async Task<ActionResult<RestDTO<RestaurantDTO>>> CreateRestaurantAsync(
            CreateRestaurantDTO dto)
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

            var createdRestaurant = await _repository.CreateAsync(restaurant);

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

            return CreatedAtAction(null, new { id = restaurant.Id }, response);
        }

        [HttpGet(Name = "GetRestaurants")]
        public async Task<ActionResult<RestDTO<RestaurantDTO[]>>> GetRestaurantsAsync(
            [FromQuery] RequestDTO input)
        {
            var (items, total) = await _repository.GetPagedAsync(input);

            var dto = items
                .Select(r => new RestaurantDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Address = r.Address,
                    PhoneNumber = r.PhoneNumber,
                    Description = r.Description
                }).ToArray();

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
                Data = dto,
                Links = links,
                RecordCount = total,
                PageIndex = input.PageIndex,
                PageSize = input.PageSize
            };



            return Ok(response);
        }

        // GET: api/Restaurants/{id}
        [HttpGet("{id}", Name = "GetRestaurant")]
        public async Task<ActionResult<RestDTO<RestaurantDTO>>> GetRestaurantAsync(
            Guid id)
        {
            var restaurant = await _repository.GetByIdAsync(id);

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

        [HttpPut("Update", Name = "UpdateRestaurant")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<RestaurantDTO?>>> UpdateRestaurantAsync(
            [FromBody] RestaurantDTO model)
        {
            // Map DTO → Entity
            var toUpdate = new Restaurant
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Description = model.Description,
                PhoneNumber = model.PhoneNumber
            };

            // Call repo
            var updated = await _repository.UpdateAsync(toUpdate);

            if (updated == null)
                return NotFound(new RestDTO<RestaurantDTO?> { Data = null });

            // Map Entity → DTO
            var dto = new RestaurantDTO
            {
                Id = updated.Id,
                Name = updated.Name,
                Address = updated.Address,
                PhoneNumber = updated.PhoneNumber,
                Description = updated.Description
            };

            // HATEOAS link
            var links = new List<LinkDTO>
            {
                new LinkDTO(
                    Url.Action(
                        null,
                        "Restaurants",
                        new { id = dto.Id },
                        Request.Scheme)!,
                    "self",
                    "POST")
            };

            return Ok(new RestDTO<RestaurantDTO?>
            {
                Data = dto,
                Links = links
            });
        }

        [HttpDelete("{id}", Name = "DeleteRestaurant")]
        public async Task<ActionResult<RestDTO<RestaurantDTO>>> DeleteRestaurantAsync(Guid id)
        {
            var restaurant = await _repository.DeleteAsync(id);

            if (restaurant == null)
                return NotFound();

            RestaurantDTO dto = new RestaurantDTO
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
                PhoneNumber = restaurant.PhoneNumber,
                Description = restaurant.Description
            };

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
