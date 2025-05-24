using Microsoft.EntityFrameworkCore;
using InventoryTracker.Core.Entities;

namespace InventoryTracker.Data.Context
{
    public class InventoryTrackerDbContext : DbContext
    {
        public InventoryTrackerDbContext(DbContextOptions<InventoryTrackerDbContext> options)
            : base(options)
        {
        }

        public DbSet<CustomerList> CustomerLists { get; set; }
        public DbSet<RfidTag> RfidTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CustomerList entity
            modelBuilder.Entity<CustomerList>(entity =>
            {
                entity.ToTable("List");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
                    
                entity.Property(e => e.SystemRef)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasMany(e => e.RfidTags)
                    .WithOne(e => e.CustomerList)
                    .HasForeignKey(e => e.ListId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure indexes
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_List_Name");
                    
                entity.HasIndex(e => e.SystemRef)
                    .HasDatabaseName("IX_List_SystemRef");
            });

            // Configure RfidTag entity
            modelBuilder.Entity<RfidTag>(entity =>
            {
                entity.ToTable("RFID");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Rfid)
                    .IsRequired()
                    .HasMaxLength(50);
                    
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
                    
                entity.Property(e => e.Color)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.Size)
                    .HasMaxLength(50);
                    
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasOne(e => e.CustomerList)
                    .WithMany(e => e.RfidTags)
                    .HasForeignKey(e => e.ListId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure indexes
                entity.HasIndex(e => e.Rfid)
                    .IsUnique()
                    .HasDatabaseName("IX_RFID_Rfid");
                    
                entity.HasIndex(e => e.ListId)
                    .HasDatabaseName("IX_RFID_ListId");
                    
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_RFID_Name");
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is CustomerList customerList)
                {
                    if (entry.State == EntityState.Added)
                    {
                        customerList.CreatedAt = DateTime.UtcNow;
                    }
                    customerList.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is RfidTag rfidTag)
                {
                    if (entry.State == EntityState.Added)
                    {
                        rfidTag.CreatedAt = DateTime.UtcNow;
                    }
                    rfidTag.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
