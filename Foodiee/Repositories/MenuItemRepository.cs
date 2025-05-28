using System.Linq.Dynamic.Core;
using Foodiee.DTO;
using Foodiee.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodiee.Repositories
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly FoodieeDbContext _context;

        public MenuItemRepository(FoodieeDbContext context)
        {
            _context = context;
        }

        public async Task<MenuItem> AddAsync(MenuItem menuItem)
        {
            var restaurant = await _context.Restaurants.FindAsync(menuItem.RestaurantId);
            if (restaurant == null)
                return null;

            menuItem.Restaurant = restaurant;
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return menuItem;
        }

        public Task<MenuItem> DeleteAsync(Guid id)
        {
            var menuItem = _context.MenuItems.FirstOrDefault(mi => mi.Id == id);
            if (menuItem == null)
                return Task.FromResult<MenuItem?>(null);

            _context.MenuItems.Remove(menuItem);
            _context.SaveChanges();
            return Task.FromResult(menuItem);


        }

        public async Task<IEnumerable<MenuItem>> GetAllAsync()
        {
            return await _context.MenuItems.ToListAsync();
        }

        public Task<MenuItem?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(
                _context.MenuItems.FirstOrDefault(mi => mi.Id == id));
        }

        public Task<(MenuItem[] Items, int TotalCount)> GetPagedAsync(Guid restaurantId, RequestDTO request)
        {
            var query = _context.MenuItems
                .Where(mi => mi.RestaurantId == restaurantId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.FilterQuery))
                query = query.Where(mi => mi.Name.Contains(request.FilterQuery));

            var totalCount = query.Count();

            var items = query
                .OrderBy($"{request.SortColumn} {request.SortOrder}")
                .Skip(request.PageIndex * request.PageSize)
                .Take(request.PageSize)
                .Select(mi => new MenuItem
                {
                    Id = mi.Id,
                    Name = mi.Name,
                    Price = mi.Price,
                    Description = mi.Description,
                    IsAvailable = mi.IsAvailable
                })
                .ToArray();

            return Task.FromResult((items, totalCount));
        }

        public async Task<MenuItem?> UpdateAsync(MenuItem menuItem)
        {
            var existingItem = _context.MenuItems.FirstOrDefault(mi => mi.Id == menuItem.Id);
            if (existingItem == null) return null;

            if (!string.IsNullOrEmpty(menuItem.Name))
                existingItem.Name = menuItem.Name;
            if (!string.IsNullOrEmpty(menuItem.Description))
                existingItem.Description = menuItem.Description;
            if (menuItem.Price > 0)
                existingItem.Price = menuItem.Price;
            if (menuItem.IsAvailable != existingItem.IsAvailable)
                existingItem.IsAvailable = menuItem.IsAvailable;

            _context.MenuItems.Update(existingItem);
            await _context.SaveChangesAsync();
            return existingItem;
        }
    }
}
