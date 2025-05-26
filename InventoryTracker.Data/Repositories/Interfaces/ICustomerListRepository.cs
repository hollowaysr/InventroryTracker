using InventoryTracker.Core.Entities;

namespace InventoryTracker.Data.Repositories.Interfaces
{    public interface ICustomerListRepository
    {
        Task<IEnumerable<CustomerList>> GetAllAsync();
        Task<CustomerList?> GetByIdAsync(Guid id);
        Task<CustomerList?> GetByIdWithTagsAsync(Guid id);
        Task<IEnumerable<CustomerList>> GetByNameAsync(string name);
        Task<CustomerList?> GetBySystemRefAsync(string systemRef);
        Task<CustomerList> CreateAsync(CustomerList customerList);
        Task<CustomerList> UpdateAsync(CustomerList customerList);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    }
}
