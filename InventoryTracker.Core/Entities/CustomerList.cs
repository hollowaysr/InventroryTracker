using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryTracker.Core.Entities
{
    /// <summary>
    /// Represents a customer list that contains RFID tags
    /// </summary>    public class CustomerList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? SystemRef { get; set; }

        // Navigation properties
        public virtual ICollection<RfidTag> RfidTags { get; set; } = new List<RfidTag>();

    // Ledger columns (from existing database)
    [Column("ledger_start_transaction_id")]
    public long LedgerStartTransactionId { get; set; }

    [Column("ledger_end_transaction_id")]
    public long? LedgerEndTransactionId { get; set; }

    [Column("ledger_start_sequence_number")]
    public long LedgerStartSequenceNumber { get; set; }    [Column("ledger_end_sequence_number")]
    public long? LedgerEndSequenceNumber { get; set; }
    }
}
