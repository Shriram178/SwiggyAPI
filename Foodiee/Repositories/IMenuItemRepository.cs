using Foodiee.DTO;
using Foodiee.Models;

namespace Foodiee.Repositories
{
    public interface IMenuItemRepository
    {
        Task<MenuItem?> GetByIdAsync(Guid id);
        Task<IEnumerable<MenuItem>> GetAllAsync();
        Task<MenuItem> AddAsync(MenuItem menuItem);
        Task<MenuItem?> UpdateAsync(MenuItem menuItem);
        Task<MenuItem> DeleteAsync(Guid id);

        Task<(MenuItem[] Items, int TotalCount)> GetPagedAsync(Guid restaurantId, RequestDTO request);
    }
}
