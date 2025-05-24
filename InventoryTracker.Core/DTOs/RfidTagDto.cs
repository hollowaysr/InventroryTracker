using System.ComponentModel.DataAnnotations;

namespace InventoryTracker.Core.DTOs
{
    public class RfidTagDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Rfid { get; set; } = string.Empty;
        
        public int ListId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Color { get; set; }
        
        [StringLength(50)]
        public string? Size { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        public string CustomerListName { get; set; } = string.Empty;
    }
    
    public class CreateRfidTagDto
    {
        [Required]
        [StringLength(50)]
        public string Rfid { get; set; } = string.Empty;
        
        [Required]
        public int ListId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Color { get; set; }
        
        [StringLength(50)]
        public string? Size { get; set; }
    }
    
    public class UpdateRfidTagDto
    {
        [Required]
        [StringLength(50)]
        public string Rfid { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Color { get; set; }
        
        [StringLength(50)]
        public string? Size { get; set; }
    }
    
    public class BulkCreateRfidTagDto
    {
        [Required]
        public int ListId { get; set; }
        
        [Required]
        [MinLength(1)]
        public List<CreateRfidTagDto> Tags { get; set; } = new();
    }
    
    // FR007: Bulk RFID adding option via comma-separated string
    public class BulkCreateFromCsvDto
    {
        [Required]
        public int ListId { get; set; }
        
        [Required]
        [StringLength(10000)]
        public string CommaSeparatedRfids { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? DefaultName { get; set; }
        
        [StringLength(500)]
        public string? DefaultDescription { get; set; }
        
        [StringLength(50)]
        public string? DefaultColor { get; set; }
        
        [StringLength(50)]
        public string? DefaultSize { get; set; }
    }

    // FR009: Export functionality
    public class ExportRfidTagsDto
    {
        [Required]
        public int ListId { get; set; }
        
        [Required]
        public ExportFormat Format { get; set; }
        
        public string? EmailAddress { get; set; }
        
        public bool IncludeMetadata { get; set; } = true;
    }
    
    public enum ExportFormat
    {
        Csv = 1,
        Excel = 2,
        Json = 3,
        Xml = 4
    }

    public class ShareRfidTagsDto
    {
        [Required]
        [MinLength(1)]
        public List<int> TagIds { get; set; } = new();
        
        [Required]
        public int TargetListId { get; set; }
        
        public bool CopyMode { get; set; } = true; // true for copy, false for move
    }
}
