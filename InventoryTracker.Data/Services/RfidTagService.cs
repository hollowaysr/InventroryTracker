using InventoryTracker.Core.DTOs;
using InventoryTracker.Core.Entities;
using InventoryTracker.Core.Services.Interfaces;
using InventoryTracker.Data.Repositories.Interfaces;
using System.Text;
using System.Linq;
using OfficeOpenXml;

namespace InventoryTracker.Data.Services
{
    public class RfidTagService : IRfidTagService
    {
        private readonly IRfidTagRepository _rfidTagRepository;
        private readonly ICustomerListRepository _customerListRepository;
        private readonly IEmailService _emailService;

        public RfidTagService(IRfidTagRepository rfidTagRepository, ICustomerListRepository customerListRepository, IEmailService emailService)
        {
            _rfidTagRepository = rfidTagRepository;
            _customerListRepository = customerListRepository;
            _emailService = emailService;
        }

        public async Task<IEnumerable<RfidTagDto>> GetAllAsync()
        {
            var rfidTags = await _rfidTagRepository.GetAllAsync();
            return rfidTags.Select(MapToDto);
        }

        public async Task<IEnumerable<RfidTagDto>> GetByListIdAsync(Guid listId)
        {
            var rfidTags = await _rfidTagRepository.GetByListIdAsync(listId);
            return rfidTags.Select(MapToDto);
        }

        public async Task<RfidTagDto?> GetByIdAsync(Guid id)
        {
            var rfidTag = await _rfidTagRepository.GetByIdAsync(id);
            return rfidTag != null ? MapToDto(rfidTag) : null;
        }

        public async Task<RfidTagDto?> GetByRfidAsync(string rfid)
        {
            var rfidTag = await _rfidTagRepository.GetByRfidAsync(rfid);
            return rfidTag != null ? MapToDto(rfidTag) : null;
        }        public async Task<RfidTagDto> CreateAsync(CreateRfidTagDto createDto)
        {
            // Validate ListId is provided
            if (!createDto.ListId.HasValue)
            {
                throw new ArgumentException("ListId is required.");
            }

            // Validate customer list exists
            if (!await _customerListRepository.ExistsAsync(createDto.ListId.Value))
            {
                throw new KeyNotFoundException($"Customer list with ID {createDto.ListId} not found.");
            }

            // Validate unique RFID
            if (await _rfidTagRepository.ExistsByRfidAsync(createDto.Rfid))
            {
                throw new InvalidOperationException($"An RFID tag with the identifier '{createDto.Rfid}' already exists.");
            }            var rfidTag = new RfidTag
            {
                Rfid = createDto.Rfid,
                ListId = createDto.ListId.Value,
                Name = createDto.Name,
                Description = createDto.Description,
                Color = createDto.Color,
                Size = createDto.Size
            };

            var createdRfidTag = await _rfidTagRepository.CreateAsync(rfidTag);
            
            // Reload with navigation properties
            var reloadedTag = await _rfidTagRepository.GetByIdAsync(createdRfidTag.Id);
            return MapToDto(reloadedTag!);
        }

        public async Task<IEnumerable<RfidTagDto>> CreateBulkAsync(BulkCreateRfidTagDto bulkCreateDto)
        {
            // Basic implementation - validate and create
            var rfidTags = bulkCreateDto.Tags.Select(createDto => new RfidTag
            {
                Rfid = createDto.Rfid,
                ListId = bulkCreateDto.ListId,
                Name = createDto.Name,
                Description = createDto.Description,
                Color = createDto.Color,
                Size = createDto.Size
            }).ToList();

            var createdRfidTags = await _rfidTagRepository.CreateBulkAsync(rfidTags);
            return createdRfidTags.Select(MapToDto);
        }

        public async Task<IEnumerable<RfidTagDto>> CreateBulkFromCsvAsync(BulkCreateFromCsvDto csvDto)
        {
            // Parse comma-separated RFIDs
            var rfidStrings = csvDto.CommaSeparatedRfids
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct()
                .ToList();

            // Create bulk create DTOs with defaults
            var createDtos = rfidStrings.Select((rfid, index) => new CreateRfidTagDto
            {
                Rfid = rfid,
                ListId = csvDto.ListId,
                Name = csvDto.DefaultName ?? $"RFID Tag {index + 1}",
                Description = csvDto.DefaultDescription,
                Color = csvDto.DefaultColor,
                Size = csvDto.DefaultSize
            }).ToList();

            var bulkCreateDto = new BulkCreateRfidTagDto
            {
                ListId = csvDto.ListId,
                Tags = createDtos
            };

            return await CreateBulkAsync(bulkCreateDto);
        }

        public async Task<RfidTagDto> UpdateAsync(Guid id, UpdateRfidTagDto updateDto)
        {
            var existingRfidTag = await _rfidTagRepository.GetByIdAsync(id);
            if (existingRfidTag == null)
            {
                throw new KeyNotFoundException($"RFID tag with ID {id} not found.");
            }

            existingRfidTag.Rfid = updateDto.Rfid;
            existingRfidTag.Name = updateDto.Name;
            existingRfidTag.Description = updateDto.Description;
            existingRfidTag.Color = updateDto.Color;
            existingRfidTag.Size = updateDto.Size;

            var updatedRfidTag = await _rfidTagRepository.UpdateAsync(existingRfidTag);
            return MapToDto(updatedRfidTag);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _rfidTagRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException($"RFID tag with ID {id} not found.");
            }

            return await _rfidTagRepository.DeleteAsync(id);
        }

        public async Task<bool> DeleteBulkAsync(IEnumerable<Guid> ids)
        {
            return await _rfidTagRepository.DeleteBulkAsync(ids);
        }        public async Task<IEnumerable<RfidTagDto>> ShareTagsAsync(ShareRfidTagsDto shareDto)
        {
            // Basic implementation - just return empty for now
            return await Task.FromResult(new List<RfidTagDto>());
        }

        public async Task<byte[]> ExportAsync(ExportRfidTagsDto exportDto)
        {
            var tags = await _rfidTagRepository.GetByListIdAsync(exportDto.ListId);
            var customerList = await _customerListRepository.GetByIdAsync(exportDto.ListId);

            return exportDto.Format switch
            {
                ExportFormat.Csv => ExportToCsv(tags, customerList!, exportDto.IncludeMetadata),
                ExportFormat.Excel => ExportToExcel(tags, customerList!, exportDto.IncludeMetadata),
                _ => ExportToCsv(tags, customerList!, exportDto.IncludeMetadata)
            };
        }

        public async Task<bool> ExportAndEmailAsync(ExportRfidTagsDto exportDto)
        {
            if (string.IsNullOrWhiteSpace(exportDto.EmailAddress))
            {
                throw new ArgumentException("Email address is required for email export.");
            }

            var exportData = await ExportAsync(exportDto);
            var customerList = await _customerListRepository.GetByIdAsync(exportDto.ListId);
            
            var subject = $"RFID Tag Export - {customerList?.Name ?? "Unknown List"}";
            var body = $"Please find attached the RFID tag export for list: {customerList?.Name ?? "Unknown List"}";
            var fileName = $"rfid_export_{exportDto.ListId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{exportDto.Format.ToString().ToLower()}";
            
            return await _emailService.SendEmailAsync(exportDto.EmailAddress, subject, body, exportData, fileName);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _rfidTagRepository.ExistsAsync(id);
        }

        private static byte[] ExportToCsv(IEnumerable<RfidTag> tags, CustomerList customerList, bool includeMetadata)
        {
            var csv = new StringBuilder();
            
            // Header
            if (includeMetadata)
            {
                csv.AppendLine("RFID,Name,Description,Color,Size");
            }
            else
            {
                csv.AppendLine("RFID");
            }
            
            // Data
            foreach (var tag in tags)
            {
                if (includeMetadata)
                {
                    csv.AppendLine($"\"{tag.Rfid}\",\"{tag.Name}\",\"{tag.Description ?? ""}\",\"{tag.Color ?? ""}\",\"{tag.Size ?? ""}\"");
                }
                else
                {
                    csv.AppendLine($"\"{tag.Rfid}\"");
                }
            }
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private static byte[] ExportToExcel(IEnumerable<RfidTag> tags, CustomerList customerList, bool includeMetadata)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("RFID Tags");
            
            // Header row
            int col = 1;
            worksheet.Cells[1, col++].Value = "RFID";
            
            if (includeMetadata)
            {
                worksheet.Cells[1, col++].Value = "Name";
                worksheet.Cells[1, col++].Value = "Description";
                worksheet.Cells[1, col++].Value = "Color";
                worksheet.Cells[1, col++].Value = "Size";
            }
            
            // Data rows
            int row = 2;
            foreach (var tag in tags)
            {
                col = 1;
                worksheet.Cells[row, col++].Value = tag.Rfid;
                
                if (includeMetadata)
                {
                    worksheet.Cells[row, col++].Value = tag.Name;
                    worksheet.Cells[row, col++].Value = tag.Description;
                    worksheet.Cells[row, col++].Value = tag.Color;
                    worksheet.Cells[row, col++].Value = tag.Size;
                }
                
                row++;
            }
            
            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
            
            return package.GetAsByteArray();
        }

        private static RfidTagDto MapToDto(RfidTag rfidTag)
        {
            return new RfidTagDto
            {
                Id = rfidTag.Id,
                Rfid = rfidTag.Rfid,
                ListId = rfidTag.ListId,
                Name = rfidTag.Name,
                Description = rfidTag.Description,
                Color = rfidTag.Color,
                Size = rfidTag.Size,
                CustomerListName = rfidTag.CustomerList?.Name ?? string.Empty
            };
        }
    }
}
