using InventoryTracker.Core.DTOs;
using InventoryTracker.Core.Entities;
using InventoryTracker.Core.Services.Interfaces;
using InventoryTracker.Data.Repositories.Interfaces;
using System.Text;
using System.Text.Json;
using OfficeOpenXml;

namespace InventoryTracker.Data.Services
{    public class RfidTagService : IRfidTagService
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
        }        public async Task<IEnumerable<RfidTagDto>> GetByListIdAsync(Guid listId)
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
        }

        public async Task<RfidTagDto> CreateAsync(CreateRfidTagDto createDto)
        {
            // Validate customer list exists
            if (!await _customerListRepository.ExistsAsync(createDto.ListId))
            {
                throw new KeyNotFoundException($"Customer list with ID {createDto.ListId} not found.");
            }

            // Validate unique RFID
            if (await _rfidTagRepository.ExistsByRfidAsync(createDto.Rfid))
            {
                throw new InvalidOperationException($"An RFID tag with the identifier '{createDto.Rfid}' already exists.");
            }

            var rfidTag = new RfidTag
            {
                Rfid = createDto.Rfid,
                ListId = createDto.ListId,
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
            // Validate customer list exists
            if (!await _customerListRepository.ExistsAsync(bulkCreateDto.ListId))
            {
                throw new KeyNotFoundException($"Customer list with ID {bulkCreateDto.ListId} not found.");
            }

            // Validate unique RFIDs
            var rfidIdentifiers = bulkCreateDto.Tags.Select(t => t.Rfid).ToList();
            var duplicateChecks = await Task.WhenAll(
                rfidIdentifiers.Select(async rfid => new { Rfid = rfid, Exists = await _rfidTagRepository.ExistsByRfidAsync(rfid) })
            );

            var duplicates = duplicateChecks.Where(x => x.Exists).Select(x => x.Rfid).ToList();
            if (duplicates.Any())
            {
                throw new InvalidOperationException($"The following RFID identifiers already exist: {string.Join(", ", duplicates)}");
            }

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
            
            // Reload with navigation properties
            var tagIds = createdRfidTags.Select(t => t.Id);
            var reloadedTags = await _rfidTagRepository.GetByIdsAsync(tagIds);
            return reloadedTags.Select(MapToDto);
        }        public async Task<RfidTagDto> UpdateAsync(Guid id, UpdateRfidTagDto updateDto)
        {
            var existingRfidTag = await _rfidTagRepository.GetByIdAsync(id);
            if (existingRfidTag == null)
            {
                throw new KeyNotFoundException($"RFID tag with ID {id} not found.");
            }

            // Validate unique RFID (excluding current record)
            if (await _rfidTagRepository.ExistsByRfidAsync(updateDto.Rfid, id))
            {
                throw new InvalidOperationException($"An RFID tag with the identifier '{updateDto.Rfid}' already exists.");
            }

            existingRfidTag.Rfid = updateDto.Rfid;
            existingRfidTag.Name = updateDto.Name;
            existingRfidTag.Description = updateDto.Description;
            existingRfidTag.Color = updateDto.Color;
            existingRfidTag.Size = updateDto.Size;

            var updatedRfidTag = await _rfidTagRepository.UpdateAsync(existingRfidTag);
            
            // Reload with navigation properties
            var reloadedTag = await _rfidTagRepository.GetByIdAsync(updatedRfidTag.Id);
            return MapToDto(reloadedTag!);
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
            var idList = ids.ToList();
            if (!idList.Any())
            {
                throw new ArgumentException("No tag IDs provided for deletion.");
            }

            var existingTags = await _rfidTagRepository.GetByIdsAsync(idList);
            var existingIds = existingTags.Select(t => t.Id).ToList();
            var missingIds = idList.Except(existingIds).ToList();

            if (missingIds.Any())
            {
                throw new KeyNotFoundException($"The following RFID tag IDs were not found: {string.Join(", ", missingIds)}");
            }

            return await _rfidTagRepository.DeleteBulkAsync(idList);
        }

        public async Task<IEnumerable<RfidTagDto>> ShareTagsAsync(ShareRfidTagsDto shareDto)
        {
            // Validate target customer list exists
            if (!await _customerListRepository.ExistsAsync(shareDto.TargetListId))
            {
                throw new KeyNotFoundException($"Target customer list with ID {shareDto.TargetListId} not found.");
            }

            // Validate source tags exist
            var sourceTags = await _rfidTagRepository.GetByIdsAsync(shareDto.TagIds);
            var existingIds = sourceTags.Select(t => t.Id).ToList();
            var missingIds = shareDto.TagIds.Except(existingIds).ToList();

            if (missingIds.Any())
            {
                throw new KeyNotFoundException($"The following RFID tag IDs were not found: {string.Join(", ", missingIds)}");
            }

            IEnumerable<RfidTag> resultTags;

            if (shareDto.CopyMode)
            {
                // Copy mode: create copies of the tags in the target list
                resultTags = await _rfidTagRepository.CopyTagsToListAsync(shareDto.TagIds, shareDto.TargetListId);
            }
            else
            {
                // Move mode: update the ListId of the existing tags
                await _rfidTagRepository.UpdateListIdBulkAsync(shareDto.TagIds, shareDto.TargetListId);
                resultTags = await _rfidTagRepository.GetByIdsAsync(shareDto.TagIds);
            }

            return resultTags.Select(MapToDto);
        }        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _rfidTagRepository.ExistsAsync(id);
        }

        // FR007: Bulk RFID adding from comma-separated string
        public async Task<IEnumerable<RfidTagDto>> CreateBulkFromCsvAsync(BulkCreateFromCsvDto csvDto)
        {
            // Validate customer list exists
            if (!await _customerListRepository.ExistsAsync(csvDto.ListId))
            {
                throw new KeyNotFoundException($"Customer list with ID {csvDto.ListId} not found.");
            }

            // Parse comma-separated RFIDs
            var rfidStrings = csvDto.CommaSeparatedRfids
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct()
                .ToList();

            if (!rfidStrings.Any())
            {
                throw new ArgumentException("No valid RFID identifiers found in the comma-separated string.");
            }

            // Validate RFID length constraints
            var invalidRfids = rfidStrings.Where(r => r.Length > 50).ToList();
            if (invalidRfids.Any())
            {
                throw new ArgumentException($"The following RFID identifiers exceed the 50 character limit: {string.Join(", ", invalidRfids)}");
            }

            // Validate unique RFIDs
            var duplicateChecks = await Task.WhenAll(
                rfidStrings.Select(async rfid => new { Rfid = rfid, Exists = await _rfidTagRepository.ExistsByRfidAsync(rfid) })
            );

            var duplicates = duplicateChecks.Where(x => x.Exists).Select(x => x.Rfid).ToList();
            if (duplicates.Any())
            {
                throw new InvalidOperationException($"The following RFID identifiers already exist: {string.Join(", ", duplicates)}");
            }

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

        // FR009: Export functionality
        public async Task<byte[]> ExportAsync(ExportRfidTagsDto exportDto)
        {
            // Validate customer list exists
            if (!await _customerListRepository.ExistsAsync(exportDto.ListId))
            {
                throw new KeyNotFoundException($"Customer list with ID {exportDto.ListId} not found.");
            }

            var tags = await _rfidTagRepository.GetByListIdAsync(exportDto.ListId);
            var customerList = await _customerListRepository.GetByIdAsync(exportDto.ListId);

            return exportDto.Format switch
            {
                ExportFormat.Csv => ExportToCsv(tags, customerList!, exportDto.IncludeMetadata),
                ExportFormat.Excel => ExportToExcel(tags, customerList!, exportDto.IncludeMetadata),
                ExportFormat.Json => ExportToJson(tags, customerList!, exportDto.IncludeMetadata),
                ExportFormat.Xml => ExportToXml(tags, customerList!, exportDto.IncludeMetadata),
                _ => throw new ArgumentException($"Unsupported export format: {exportDto.Format}")
            };
        }        public async Task<bool> ExportAndEmailAsync(ExportRfidTagsDto exportDto)
        {
            if (string.IsNullOrWhiteSpace(exportDto.EmailAddress))
            {
                throw new ArgumentException("Email address is required for email export.");
            }

            var exportData = await ExportAsync(exportDto);
            var customerList = await _customerListRepository.GetByIdAsync(exportDto.ListId);
            
            var subject = $"RFID Tag Export - {customerList?.Name ?? "Unknown List"}";
            var body = $"Please find attached the RFID tag export for list: {customerList?.Name ?? "Unknown List"}\n\n" +
                      $"Export Format: {exportDto.Format}\n" +
                      $"Include Metadata: {exportDto.IncludeMetadata}\n" +
                      $"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
            
            var fileName = $"rfid_export_{exportDto.ListId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{exportDto.Format.ToString().ToLower()}";
            
            return await _emailService.SendEmailAsync(exportDto.EmailAddress, subject, body, exportData, fileName);
        }

        private static byte[] ExportToCsv(IEnumerable<RfidTag> tags, CustomerList customerList, bool includeMetadata)
        {
            var csv = new System.Text.StringBuilder();
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
                if (includeMetadata)                // Note: Timestamp fields removed for TestApps database compatibility                {
                    csv.AppendLine($"\"{tag.Rfid}\",\"{tag.Name}\",\"{tag.Description ?? ""}\",\"{tag.Color ?? ""}\",\"{tag.Size ?? ""}\"");
                }
                else
                {
                    csv.AppendLine($"\"{tag.Rfid}\"");
                }
                }
            }
            
            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }        private static byte[] ExportToExcel(IEnumerable<RfidTag> tags, CustomerList customerList, bool includeMetadata)
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
                // Note: Timestamp fields removed for TestApps database compatibility
            }
            
            // Format header
            var headerRange = worksheet.Cells[1, 1, 1, col - 1];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            
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
                    worksheet.Cells[row, col++].Value = tag.Size;                    // Note: Timestamp fields removed for TestApps database compatibility
                }
                
                row++;
            }
            
            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
            
            // Add customer list info as a comment or separate sheet info
            worksheet.Cells[1, 1].AddComment($"Customer List: {customerList.Name}\nDescription: {customerList.Description}\nExported: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC", "RFID Tracker");
            
            return package.GetAsByteArray();
        }

        private static byte[] ExportToJson(IEnumerable<RfidTag> tags, CustomerList customerList, bool includeMetadata)
        {
            var exportData = new
            {
                CustomerList = new
                {
                    customerList.Id,
                    customerList.Name,
                    customerList.Description,
                    customerList.SystemRef
                },                Tags = tags.Select(tag => includeMetadata ? 
                    new
                    {
                        tag.Rfid,
                        tag.Name,
                        tag.Description,
                        tag.Color,
                        tag.Size
                        // Note: Timestamp fields removed for TestApps database compatibility
                    } :
                    new { tag.Rfid }).ToList(),
                ExportedAt = DateTime.UtcNow
            };

            var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        private static byte[] ExportToXml(IEnumerable<RfidTag> tags, CustomerList customerList, bool includeMetadata)
        {
            var xml = new System.Text.StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<RfidExport>");
            xml.AppendLine($"  <CustomerList>");
            xml.AppendLine($"    <Id>{customerList.Id}</Id>");
            xml.AppendLine($"    <Name><![CDATA[{customerList.Name}]]></Name>");
            xml.AppendLine($"    <Description><![CDATA[{customerList.Description ?? ""}]]></Description>");
            xml.AppendLine($"    <SystemRef><![CDATA[{customerList.SystemRef ?? ""}]]></SystemRef>");
            xml.AppendLine($"  </CustomerList>");
            xml.AppendLine("  <Tags>");
            
            foreach (var tag in tags)
            {
                xml.AppendLine("    <Tag>");
                xml.AppendLine($"      <Rfid><![CDATA[{tag.Rfid}]]></Rfid>");
                
                if (includeMetadata)
                {
                    xml.AppendLine($"      <Name><![CDATA[{tag.Name}]]></Name>");
                    xml.AppendLine($"      <Description><![CDATA[{tag.Description ?? ""}]]></Description>");
                    xml.AppendLine($"      <Color><![CDATA[{tag.Color ?? ""}]]></Color>");
                    xml.AppendLine($"      <Size><![CDATA[{tag.Size ?? ""}]]></Size>");                // Note: Timestamp fields removed for TestApps database compatibility
                }
                
                xml.AppendLine("    </Tag>");
            }
            
            xml.AppendLine("  </Tags>");
            xml.AppendLine($"  <ExportedAt>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}</ExportedAt>");
            xml.AppendLine("</RfidExport>");
            
            return System.Text.Encoding.UTF8.GetBytes(xml.ToString());
        }        private static RfidTagDto MapToDto(RfidTag rfidTag)
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
