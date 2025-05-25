using Microsoft.AspNetCore.Mvc;
using InventoryTracker.Core.DTOs;
using InventoryTracker.Core.Services.Interfaces;

namespace InventoryTracker.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerListsController : ControllerBase
    {
        private readonly ICustomerListService _customerListService;
        private readonly ILogger<CustomerListsController> _logger;

        public CustomerListsController(ICustomerListService customerListService, ILogger<CustomerListsController> logger)
        {
            _customerListService = customerListService;
            _logger = logger;
        }

        /// <summary>
        /// Get all customer lists (FR001)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerListDto>>> GetAll()
        {
            try
            {
                var customerLists = await _customerListService.GetAllAsync();
                return Ok(customerLists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer lists");
                return StatusCode(500, "An error occurred while retrieving customer lists");
            }
        }        /// <summary>
        /// Get a specific customer list by ID (FR002)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerListDto>> GetById(Guid id)
        {
            try
            {
                var customerList = await _customerListService.GetByIdAsync(id);
                if (customerList == null)
                {
                    return NotFound($"Customer list with ID {id} not found");
                }
                return Ok(customerList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer list with ID {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the customer list");
            }
        }        /// <summary>
        /// Get a specific customer list with its RFID tags (FR002)
        /// </summary>
        [HttpGet("{id}/with-tags")]
        public async Task<ActionResult<CustomerListDto>> GetByIdWithTags(Guid id)
        {
            try
            {
                var customerList = await _customerListService.GetByIdWithTagsAsync(id);
                if (customerList == null)
                {
                    return NotFound($"Customer list with ID {id} not found");
                }
                return Ok(customerList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer list with tags for ID {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the customer list with tags");
            }
        }

        /// <summary>
        /// Create a new customer list (FR003)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CustomerListDto>> Create([FromBody] CreateCustomerListDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var customerList = await _customerListService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = customerList.Id }, customerList);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating customer list");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer list");
                return StatusCode(500, "An error occurred while creating the customer list");
            }
        }        /// <summary>
        /// Update an existing customer list (FR004)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerListDto>> Update(Guid id, [FromBody] UpdateCustomerListDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var customerList = await _customerListService.UpdateAsync(id, updateDto);
                return Ok(customerList);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer list not found for update");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating customer list");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer list with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the customer list");
            }
        }        /// <summary>
        /// Delete a customer list (FR005)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _customerListService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Customer list with ID {id} not found");
                }
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Customer list not found for deletion");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer list with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the customer list");
            }
        }
    }
}
