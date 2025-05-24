using InventoryTracker.Core.Entities;

namespace InventoryTracker.Data.Repositories.Interfaces
{
    public interface IRfidTagRepository
    {
        Task<IEnumerable<RfidTag>> GetAllAsync();
        Task<IEnumerable<RfidTag>> GetByListIdAsync(int listId);
        Task<RfidTag?> GetByIdAsync(int id);
        Task<RfidTag?> GetByRfidAsync(string rfid);
        Task<RfidTag> CreateAsync(RfidTag rfidTag);
        Task<IEnumerable<RfidTag>> CreateBulkAsync(IEnumerable<RfidTag> rfidTags);
        Task<RfidTag> UpdateAsync(RfidTag rfidTag);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteBulkAsync(IEnumerable<int> ids);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByRfidAsync(string rfid, int? excludeId = null);
        Task<IEnumerable<RfidTag>> GetByIdsAsync(IEnumerable<int> ids);
        Task<bool> UpdateListIdBulkAsync(IEnumerable<int> tagIds, int targetListId);
        Task<IEnumerable<RfidTag>> CopyTagsToListAsync(IEnumerable<int> tagIds, int targetListId);
    }
}
