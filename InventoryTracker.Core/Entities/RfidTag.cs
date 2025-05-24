using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryTracker.Core.Entities
{
    /// <summary>
    /// Represents an RFID tag with associated metadata
    /// </summary>
    public class RfidTag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("RFID")]
        public string Rfid { get; set; } = string.Empty;

        [ForeignKey(nameof(CustomerList))]
        public int ListId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual CustomerList? CustomerList { get; set; }

        // Ledger columns (from existing database)
        [Column("ledger_start_transaction_id")]
        public long LedgerStartTransactionId { get; set; }

        [Column("ledger_end_transaction_id")]
        public long? LedgerEndTransactionId { get; set; }

        [Column("ledger_start_sequence_number")]
        public long LedgerStartSequenceNumber { get; set; }

        [Column("ledger_end_sequence_number")]
        public long? LedgerEndSequenceNumber { get; set; }
    }
}
