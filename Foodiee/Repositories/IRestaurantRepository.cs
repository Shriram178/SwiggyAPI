using Foodiee.DTO;
using Foodiee.Models;

namespace Foodiee.Repositories
{
    public interface IRestaurantRepository
    {
        Task<List<Restaurant>> GetAllAsync();
        Task<Restaurant?> GetByIdAsync(Guid id);
        Task<Restaurant> CreateAsync(Restaurant restaurant);

        Task<(Restaurant[] Items, int TotalCount)> GetPagedAsync(RequestDTO request);

        Task<Restaurant?> UpdateAsync(Restaurant toUpdate);

        Task<Restaurant?> DeleteAsync(Guid id);
    }
}

