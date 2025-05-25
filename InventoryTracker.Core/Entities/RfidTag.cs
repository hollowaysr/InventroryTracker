using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryTracker.Core.Entities
{
    /// <summary>
    /// Represents an RFID tag with associated metadata
    /// </summary>
    public class RfidTag
    {        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("RFID", TypeName = "varchar")]
        public string Rfid { get; set; } = string.Empty;

        [ForeignKey(nameof(CustomerList))]
        public Guid? ListId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string? Name { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string? Size { get; set; }

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
