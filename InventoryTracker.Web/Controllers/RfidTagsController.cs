using Microsoft.AspNetCore.Mvc;
using InventoryTracker.Core.DTOs;
using InventoryTracker.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace InventoryTracker.Web.Controllers
{    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireInventoryRole")]
    public class RfidTagsController : ControllerBase
    {
        private readonly IRfidTagService _rfidTagService;
        private readonly ILogger<RfidTagsController> _logger;

        public RfidTagsController(IRfidTagService rfidTagService, ILogger<RfidTagsController> logger)
        {
            _rfidTagService = rfidTagService;
            _logger = logger;
        }

        /// <summary>
        /// Get all RFID tags (FR006)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RfidTagDto>>> GetAll()
        {
            try
            {
                var rfidTags = await _rfidTagService.GetAllAsync();
                return Ok(rfidTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving RFID tags");
                return StatusCode(500, "An error occurred while retrieving RFID tags");
            }
        }        /// <summary>
        /// Get RFID tags by customer list ID (FR006)
        /// </summary>
        [HttpGet("by-list/{listId}")]
        public async Task<ActionResult<IEnumerable<RfidTagDto>>> GetByListId(Guid listId)
        {
            try
            {
                var rfidTags = await _rfidTagService.GetByListIdAsync(listId);
                return Ok(rfidTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving RFID tags for list ID {ListId}", listId);
                return StatusCode(500, "An error occurred while retrieving RFID tags");
            }
        }        /// <summary>
        /// Get a specific RFID tag by ID (FR007)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RfidTagDto>> GetById(Guid id)
        {
            try
            {
                var rfidTag = await _rfidTagService.GetByIdAsync(id);
                if (rfidTag == null)
                {
                    return NotFound($"RFID tag with ID {id} not found");
                }
                return Ok(rfidTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving RFID tag with ID {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the RFID tag");
            }
        }

        /// <summary>
        /// Get a specific RFID tag by RFID identifier (FR007)
        /// </summary>
        [HttpGet("by-rfid/{rfid}")]
        public async Task<ActionResult<RfidTagDto>> GetByRfid(string rfid)
        {
            try
            {
                var rfidTag = await _rfidTagService.GetByRfidAsync(rfid);
                if (rfidTag == null)
                {
                    return NotFound($"RFID tag with identifier '{rfid}' not found");
                }
                return Ok(rfidTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving RFID tag with identifier {Rfid}", rfid);
                return StatusCode(500, "An error occurred while retrieving the RFID tag");
            }
        }

        /// <summary>
        /// Create a new RFID tag (FR008)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RfidTagDto>> Create([FromBody] CreateRfidTagDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var rfidTag = await _rfidTagService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = rfidTag.Id }, rfidTag);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer list not found for RFID tag creation");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating RFID tag");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RFID tag");
                return StatusCode(500, "An error occurred while creating the RFID tag");
            }
        }

        /// <summary>
        /// Create multiple RFID tags in bulk (FR010)
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<RfidTagDto>>> CreateBulk([FromBody] BulkCreateRfidTagDto bulkCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var rfidTags = await _rfidTagService.CreateBulkAsync(bulkCreateDto);
                return CreatedAtAction(nameof(GetByListId), new { listId = bulkCreateDto.ListId }, rfidTags);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer list not found for bulk RFID tag creation");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating RFID tags in bulk");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RFID tags in bulk");
                return StatusCode(500, "An error occurred while creating RFID tags in bulk");
            }
        }        /// <summary>
        /// Update an existing RFID tag (FR009)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<RfidTagDto>> Update(Guid id, [FromBody] UpdateRfidTagDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var rfidTag = await _rfidTagService.UpdateAsync(id, updateDto);
                return Ok(rfidTag);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "RFID tag not found for update");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating RFID tag");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating RFID tag with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the RFID tag");
            }
        }        /// <summary>
        /// Delete an RFID tag (FR009)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _rfidTagService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"RFID tag with ID {id} not found");
                }
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "RFID tag not found for deletion");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting RFID tag with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the RFID tag");
            }
        }        /// <summary>
        /// Delete multiple RFID tags in bulk (FR011)
        /// </summary>
        [HttpDelete("bulk")]
        public async Task<ActionResult> DeleteBulk([FromBody] IEnumerable<Guid> ids)
        {
            try
            {
                var idList = ids.ToList();
                if (!idList.Any())
                {
                    return BadRequest("No tag IDs provided for deletion");
                }

                var result = await _rfidTagService.DeleteBulkAsync(idList);
                if (!result)
                {
                    return BadRequest("No tags were deleted");
                }
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid arguments for bulk deletion");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Some RFID tags not found for bulk deletion");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting RFID tags in bulk");
                return StatusCode(500, "An error occurred while deleting RFID tags in bulk");
            }
        }

        /// <summary>
        /// Share RFID tags between customer lists (FR012)
        /// </summary>
        [HttpPost("share")]
        public async Task<ActionResult<IEnumerable<RfidTagDto>>> ShareTags([FromBody] ShareRfidTagsDto shareDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var sharedTags = await _rfidTagService.ShareTagsAsync(shareDto);
                return Ok(sharedTags);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer list or RFID tags not found for sharing");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing RFID tags");
                return StatusCode(500, "An error occurred while sharing RFID tags");
            }
        }

        /// <summary>
        /// Bulk create RFID tags from comma-separated string (FR007)
        /// </summary>
        [HttpPost("bulk-csv")]
        public async Task<ActionResult<IEnumerable<RfidTagDto>>> CreateBulkFromCsv([FromBody] BulkCreateFromCsvDto csvDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdTags = await _rfidTagService.CreateBulkFromCsvAsync(csvDto);
                return CreatedAtAction(nameof(GetByListId), new { listId = csvDto.ListId }, createdTags);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer list not found for bulk CSV creation");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid CSV data for bulk creation");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Duplicate RFID identifiers in bulk CSV creation");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RFID tags from CSV");
                return StatusCode(500, "An error occurred while creating RFID tags from CSV");
            }
        }

        /// <summary>
        /// Export RFID tags from a customer list (FR009)
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportTags([FromBody] ExportRfidTagsDto exportDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var exportData = await _rfidTagService.ExportAsync(exportDto);
                
                var contentType = exportDto.Format switch
                {
                    ExportFormat.Csv => "text/csv",
                    ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ExportFormat.Json => "application/json",
                    ExportFormat.Xml => "application/xml",
                    _ => "application/octet-stream"
                };

                var fileExtension = exportDto.Format switch
                {
                    ExportFormat.Csv => "csv",
                    ExportFormat.Excel => "xlsx",
                    ExportFormat.Json => "json",
                    ExportFormat.Xml => "xml",
                    _ => "dat"
                };

                var fileName = $"rfid-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{fileExtension}";
                
                return File(exportData, contentType, fileName);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer list not found for export");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid export parameters");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting RFID tags");
                return StatusCode(500, "An error occurred while exporting RFID tags");
            }
        }

        /// <summary>
        /// Export and email RFID tags from a customer list (FR009)
        /// </summary>
        [HttpPost("export-email")]
        public async Task<IActionResult> ExportAndEmailTags([FromBody] ExportRfidTagsDto exportDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(exportDto.EmailAddress))
                {
                    return BadRequest("Email address is required for email export");
                }

                var success = await _rfidTagService.ExportAndEmailAsync(exportDto);
                
                if (success)
                {
                    return Ok(new { message = "Export sent successfully", emailAddress = exportDto.EmailAddress });
                }
                else
                {
                    return StatusCode(500, "Failed to send export via email");
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer list not found for email export");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid email export parameters");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting and emailing RFID tags");
                return StatusCode(500, "An error occurred while exporting and emailing RFID tags");
            }
        }
    }
}
