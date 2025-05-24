using InventoryTracker.Core.DTOs;

namespace InventoryTracker.Core.Services.Interfaces
{
    public interface ICustomerListService
    {
        Task<IEnumerable<CustomerListDto>> GetAllAsync();
        Task<CustomerListDto?> GetByIdAsync(int id);
        Task<CustomerListDto?> GetByIdWithTagsAsync(int id);
        Task<CustomerListDto> CreateAsync(CreateCustomerListDto createDto);
        Task<CustomerListDto> UpdateAsync(int id, UpdateCustomerListDto updateDto);
        Task<bool> DeleteAsync(int id);        Task<bool> ExistsAsync(int id);
    }
}
