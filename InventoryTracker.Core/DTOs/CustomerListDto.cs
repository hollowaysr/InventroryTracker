using System.ComponentModel.DataAnnotations;

namespace InventoryTracker.Core.DTOs
{

/// <summary>
/// Data transfer object for creating a new customer list
/// </summary>
public class CreateCustomerListDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? SystemRef { get; set; }
}

/// <summary>
/// Data transfer object for updating a customer list
/// </summary>
public class UpdateCustomerListDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? SystemRef { get; set; }
}

/// <summary>
/// Data transfer object for customer list response
/// </summary>
public class CustomerListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SystemRef { get; set; }
    public int TagCount { get; set; }
}
}
