using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");
        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.MovementType)
            .IsRequired();

        builder.Property(sm => sm.Quantity)
            .IsRequired();

        builder.Property(sm => sm.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(sm => sm.Notes)
            .HasMaxLength(500);

        builder.Property(sm => sm.CreatedBy)
            .HasMaxLength(100);

        builder.HasIndex(sm => new { sm.StoreId, sm.InventoryItemId });
        builder.HasIndex(sm => new { sm.StoreId, sm.CreatedAt });

        builder.HasOne(sm => sm.Store)
            .WithMany()
            .HasForeignKey(sm => sm.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.InventoryItem)
            .WithMany(i => i.StockMovements)
            .HasForeignKey(sm => sm.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
