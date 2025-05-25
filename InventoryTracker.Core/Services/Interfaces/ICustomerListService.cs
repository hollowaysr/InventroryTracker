using InventoryTracker.Core.DTOs;

namespace InventoryTracker.Core.Services.Interfaces
{    public interface ICustomerListService
    {
        Task<IEnumerable<CustomerListDto>> GetAllAsync();
        Task<CustomerListDto?> GetByIdAsync(Guid id);
        Task<CustomerListDto?> GetByIdWithTagsAsync(Guid id);
        Task<CustomerListDto> CreateAsync(CreateCustomerListDto createDto);
        Task<CustomerListDto> UpdateAsync(Guid id, UpdateCustomerListDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
