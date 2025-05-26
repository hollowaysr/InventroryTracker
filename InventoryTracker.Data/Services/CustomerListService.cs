using InventoryTracker.Core.DTOs;
using InventoryTracker.Core.Entities;
using InventoryTracker.Core.Services.Interfaces;
using InventoryTracker.Data.Repositories.Interfaces;
using System.Linq;

namespace InventoryTracker.Data.Services
{
    public class CustomerListService : ICustomerListService
    {
        private readonly ICustomerListRepository _customerListRepository;

        public CustomerListService(ICustomerListRepository customerListRepository)
        {
            _customerListRepository = customerListRepository;
        }        public async Task<IEnumerable<CustomerListDto>> GetAllAsync()
        {
            var customerLists = await _customerListRepository.GetAllAsync();
            return customerLists.Select(MapToDto);
        }        public async Task<CustomerListDto?> GetByIdAsync(Guid id)
        {
            var customerList = await _customerListRepository.GetByIdAsync(id);
            return customerList != null ? MapToDto(customerList) : null;
        }

        public async Task<CustomerListDto?> GetByIdWithTagsAsync(Guid id)
        {
            var customerList = await _customerListRepository.GetByIdWithTagsAsync(id);
            return customerList != null ? MapToDtoWithTags(customerList) : null;
        }

        public async Task<IEnumerable<CustomerListDto>> GetByNameAsync(string name)
        {
            var customerLists = await _customerListRepository.GetByNameAsync(name);
            return customerLists.Select(MapToDto);
        }

        public async Task<CustomerListDto> CreateAsync(CreateCustomerListDto createDto)
        {
            // Validate unique name
            if (await _customerListRepository.ExistsByNameAsync(createDto.Name))
            {
                throw new InvalidOperationException($"A customer list with the name '{createDto.Name}' already exists.");
            }

            var customerList = new CustomerList
            {
                Name = createDto.Name,
                Description = createDto.Description,
                SystemRef = createDto.SystemRef
            };            var createdCustomerList = await _customerListRepository.CreateAsync(customerList);
            return MapToDto(createdCustomerList);
        }

        public async Task<CustomerListDto> UpdateAsync(Guid id, UpdateCustomerListDto updateDto)
        {
            var existingCustomerList = await _customerListRepository.GetByIdAsync(id);
            if (existingCustomerList == null)
            {
                throw new KeyNotFoundException($"Customer list with ID {id} not found.");
            }

            // Validate unique name (excluding current record)
            if (await _customerListRepository.ExistsByNameAsync(updateDto.Name, id))
            {
                throw new InvalidOperationException($"A customer list with the name '{updateDto.Name}' already exists.");
            }

            existingCustomerList.Name = updateDto.Name;
            existingCustomerList.Description = updateDto.Description;
            existingCustomerList.SystemRef = updateDto.SystemRef;

            var updatedCustomerList = await _customerListRepository.UpdateAsync(existingCustomerList);
            return MapToDto(updatedCustomerList);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _customerListRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException($"Customer list with ID {id} not found.");
            }

            return await _customerListRepository.DeleteAsync(id);
        }        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _customerListRepository.ExistsAsync(id);
        }

        private static CustomerListDto MapToDto(CustomerList customerList)
        {
            return new CustomerListDto
            {
                Id = customerList.Id,
                Name = customerList.Name,
                Description = customerList.Description,
                SystemRef = customerList.SystemRef,
                TagCount = customerList.RfidTags?.Count ?? 0
            };
        }

        private static CustomerListDto MapToDtoWithTags(CustomerList customerList)
        {
            var dto = MapToDto(customerList);
            dto.TagCount = customerList.RfidTags?.Count ?? 0;
            return dto;
        }
    }
}
