using Microsoft.EntityFrameworkCore;
using InventoryTracker.Core.Entities;
using InventoryTracker.Data.Context;
using InventoryTracker.Data.Repositories.Interfaces;

namespace InventoryTracker.Data.Repositories
{
    public class CustomerListRepository : ICustomerListRepository
    {
        private readonly InventoryTrackerDbContext _context;

        public CustomerListRepository(InventoryTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerList>> GetAllAsync()
        {
            return await _context.CustomerLists
                .Include(cl => cl.RfidTags)
                .OrderBy(cl => cl.Name)
                .ToListAsync();
        }        public async Task<CustomerList?> GetByIdAsync(Guid id)
        {
            return await _context.CustomerLists
                .FirstOrDefaultAsync(cl => cl.Id == id);
        }

        public async Task<CustomerList?> GetByIdWithTagsAsync(Guid id)
        {
            return await _context.CustomerLists
                .Include(cl => cl.RfidTags)
                .FirstOrDefaultAsync(cl => cl.Id == id);
        }

        public async Task<CustomerList> CreateAsync(CustomerList customerList)
        {
            _context.CustomerLists.Add(customerList);
            await _context.SaveChangesAsync();
            return customerList;
        }

        public async Task<CustomerList> UpdateAsync(CustomerList customerList)
        {
            _context.CustomerLists.Update(customerList);
            await _context.SaveChangesAsync();
            return customerList;
        }        public async Task<bool> DeleteAsync(Guid id)
        {
            var customerList = await _context.CustomerLists.FindAsync(id);
            if (customerList == null)
                return false;

            _context.CustomerLists.Remove(customerList);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.CustomerLists.AnyAsync(cl => cl.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            var query = _context.CustomerLists.Where(cl => cl.Name == name);
            
            if (excludeId.HasValue)
            {
                query = query.Where(cl => cl.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }
    }
}
