using InventoryTracker.Core.Entities;

namespace InventoryTracker.Data.Repositories.Interfaces
{    public interface IRfidTagRepository
    {
        Task<IEnumerable<RfidTag>> GetAllAsync();
        Task<IEnumerable<RfidTag>> GetByListIdAsync(Guid listId);
        Task<RfidTag?> GetByIdAsync(Guid id);
        Task<RfidTag?> GetByRfidAsync(string rfid);
        Task<IEnumerable<RfidTag>> GetByNameAsync(string name);
        Task<RfidTag> CreateAsync(RfidTag rfidTag);
        Task<IEnumerable<RfidTag>> CreateBulkAsync(IEnumerable<RfidTag> rfidTags);
        Task<RfidTag> UpdateAsync(RfidTag rfidTag);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteBulkAsync(IEnumerable<Guid> ids);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByRfidAsync(string rfid, Guid? excludeId = null);
        Task<IEnumerable<RfidTag>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<bool> UpdateListIdBulkAsync(IEnumerable<Guid> tagIds, Guid targetListId);
        Task<IEnumerable<RfidTag>> CopyTagsToListAsync(IEnumerable<Guid> tagIds, Guid targetListId);
    }
}
