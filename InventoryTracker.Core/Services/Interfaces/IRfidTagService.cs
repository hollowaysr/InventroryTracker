using InventoryTracker.Core.DTOs;

namespace InventoryTracker.Core.Services.Interfaces
{    public interface IRfidTagService
    {
        Task<IEnumerable<RfidTagDto>> GetAllAsync();
        Task<IEnumerable<RfidTagDto>> GetByListIdAsync(Guid listId);
        Task<RfidTagDto?> GetByIdAsync(Guid id);
        Task<RfidTagDto?> GetByRfidAsync(string rfid);
        Task<RfidTagDto> CreateAsync(CreateRfidTagDto createDto);
        Task<IEnumerable<RfidTagDto>> CreateBulkAsync(BulkCreateRfidTagDto bulkCreateDto);
        
        // FR007: Bulk RFID adding from comma-separated string
        Task<IEnumerable<RfidTagDto>> CreateBulkFromCsvAsync(BulkCreateFromCsvDto csvDto);
        
        Task<RfidTagDto> UpdateAsync(Guid id, UpdateRfidTagDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteBulkAsync(IEnumerable<Guid> ids);
        Task<IEnumerable<RfidTagDto>> ShareTagsAsync(ShareRfidTagsDto shareDto);
        
        // FR009: Export functionality
        Task<byte[]> ExportAsync(ExportRfidTagsDto exportDto);
        Task<bool> ExportAndEmailAsync(ExportRfidTagsDto exportDto);
        
        Task<bool> ExistsAsync(Guid id);
    }
}
