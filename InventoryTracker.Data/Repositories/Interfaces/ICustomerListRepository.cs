using InventoryTracker.Core.Entities;

namespace InventoryTracker.Data.Repositories.Interfaces
{
    public interface ICustomerListRepository
    {
        Task<IEnumerable<CustomerList>> GetAllAsync();
        Task<CustomerList?> GetByIdAsync(int id);
        Task<CustomerList?> GetByIdWithTagsAsync(int id);
        Task<CustomerList> CreateAsync(CustomerList customerList);
        Task<CustomerList> UpdateAsync(CustomerList customerList);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    }
}
