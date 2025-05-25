using Microsoft.EntityFrameworkCore;
using InventoryTracker.Core.Entities;
using InventoryTracker.Data.Context;
using InventoryTracker.Data.Repositories.Interfaces;

namespace InventoryTracker.Data.Repositories
{
    public class RfidTagRepository : IRfidTagRepository
    {
        private readonly InventoryTrackerDbContext _context;

        public RfidTagRepository(InventoryTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RfidTag>> GetAllAsync()
        {
            return await _context.RfidTags
                .Include(rt => rt.CustomerList)
                .OrderBy(rt => rt.Name)
                .ToListAsync();
        }        public async Task<IEnumerable<RfidTag>> GetByListIdAsync(Guid listId)
        {
            return await _context.RfidTags
                .Include(rt => rt.CustomerList)
                .Where(rt => rt.ListId == listId)
                .OrderBy(rt => rt.Name)
                .ToListAsync();
        }

        public async Task<RfidTag?> GetByIdAsync(Guid id)
        {
            return await _context.RfidTags
                .Include(rt => rt.CustomerList)
                .FirstOrDefaultAsync(rt => rt.Id == id);
        }

        public async Task<RfidTag?> GetByRfidAsync(string rfid)
        {
            return await _context.RfidTags
                .Include(rt => rt.CustomerList)
                .FirstOrDefaultAsync(rt => rt.Rfid == rfid);
        }

        public async Task<RfidTag> CreateAsync(RfidTag rfidTag)
        {
            _context.RfidTags.Add(rfidTag);
            await _context.SaveChangesAsync();
            return rfidTag;
        }

        public async Task<IEnumerable<RfidTag>> CreateBulkAsync(IEnumerable<RfidTag> rfidTags)
        {
            _context.RfidTags.AddRange(rfidTags);
            await _context.SaveChangesAsync();
            return rfidTags;
        }

        public async Task<RfidTag> UpdateAsync(RfidTag rfidTag)
        {
            _context.RfidTags.Update(rfidTag);
            await _context.SaveChangesAsync();
            return rfidTag;
        }        public async Task<bool> DeleteAsync(Guid id)
        {
            var rfidTag = await _context.RfidTags.FindAsync(id);
            if (rfidTag == null)
                return false;

            _context.RfidTags.Remove(rfidTag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBulkAsync(IEnumerable<Guid> ids)
        {
            var rfidTags = await _context.RfidTags
                .Where(rt => ids.Contains(rt.Id))
                .ToListAsync();

            if (!rfidTags.Any())
                return false;

            _context.RfidTags.RemoveRange(rfidTags);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.RfidTags.AnyAsync(rt => rt.Id == id);
        }

        public async Task<bool> ExistsByRfidAsync(string rfid, Guid? excludeId = null)
        {
            var query = _context.RfidTags.Where(rt => rt.Rfid == rfid);
            
            if (excludeId.HasValue)
            {
                query = query.Where(rt => rt.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<RfidTag>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.RfidTags
                .Include(rt => rt.CustomerList)
                .Where(rt => ids.Contains(rt.Id))
                .ToListAsync();
        }

        public async Task<bool> UpdateListIdBulkAsync(IEnumerable<Guid> tagIds, Guid targetListId)
        {
            var tags = await _context.RfidTags
                .Where(rt => tagIds.Contains(rt.Id))
                .ToListAsync();

            if (!tags.Any())
                return false;

            foreach (var tag in tags)
            {
                tag.ListId = targetListId;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RfidTag>> CopyTagsToListAsync(IEnumerable<Guid> tagIds, Guid targetListId)
        {
            var originalTags = await _context.RfidTags
                .Where(rt => tagIds.Contains(rt.Id))
                .ToListAsync();

            if (!originalTags.Any())
                return Enumerable.Empty<RfidTag>();

            var copiedTags = originalTags.Select(tag => new RfidTag
            {
                Rfid = GenerateUniqueRfid(tag.Rfid),
                ListId = targetListId,
                Name = tag.Name,
                Description = tag.Description,
                Color = tag.Color,
                Size = tag.Size
            }).ToList();

            _context.RfidTags.AddRange(copiedTags);
            await _context.SaveChangesAsync();
            return copiedTags;
        }

        private string GenerateUniqueRfid(string originalRfid)
        {
            // Generate a unique RFID by appending a timestamp or counter
            // This is a simple implementation - in production, you might want a more sophisticated approach
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"{originalRfid}_COPY_{timestamp}";
        }
    }
}
