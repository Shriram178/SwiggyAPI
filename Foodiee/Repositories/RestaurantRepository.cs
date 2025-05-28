using System.Linq.Dynamic.Core;
using Foodiee.DTO;
using Foodiee.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodiee.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly FoodieeDbContext _context;

        public RestaurantRepository(FoodieeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Restaurant>> GetAllAsync()
        {
            return await _context.Restaurants.ToListAsync();
        }

        public async Task<Restaurant?> GetByIdAsync(Guid id)
        {
            return await _context.Restaurants.FindAsync(id);
        }

        public async Task<Restaurant> CreateAsync(Restaurant restaurant)
        {
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            return restaurant;
        }

        public async Task<(Restaurant[] Items, int TotalCount)> GetPagedAsync(RequestDTO input)
        {
            var query = _context.Restaurants.AsQueryable();

            if (!string.IsNullOrEmpty(input.FilterQuery))
                query = query.Where(r => r.Name.Contains(input.FilterQuery));

            var total = await query.CountAsync();

            var page = await query
                .OrderBy($"{input.SortColumn} {input.SortOrder}")
                .Skip(input.PageIndex * input.PageSize)
                .Take(input.PageSize)
                .Select(r => new Restaurant
                {
                    Id = r.Id,
                    Name = r.Name,
                    Address = r.Address,
                    PhoneNumber = r.PhoneNumber,
                    Description = r.Description
                })
                .ToArrayAsync();

            return (page, total);
        }

        public async Task<Restaurant?> UpdateAsync(Restaurant toUpdate)
        {
            var existing = await _context.Restaurants.FindAsync(toUpdate.Id);
            if (existing == null) return null;

            // Patch only non-empty values
            if (!string.IsNullOrEmpty(toUpdate.Name)) existing.Name = toUpdate.Name;
            if (!string.IsNullOrEmpty(toUpdate.Address)) existing.Address = toUpdate.Address;
            if (!string.IsNullOrEmpty(toUpdate.Description)) existing.Description = toUpdate.Description;
            if (!string.IsNullOrEmpty(toUpdate.PhoneNumber)) existing.PhoneNumber = toUpdate.PhoneNumber;

            _context.Restaurants.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public Task<Restaurant?> DeleteAsync(Guid id)
        {
            var restaurant = _context.Restaurants.Find(id);
            if (restaurant == null) return Task.FromResult<Restaurant?>(null);
            _context.Restaurants.Remove(restaurant);
            _context.SaveChanges();
            return Task.FromResult(restaurant);
        }
    }
}
