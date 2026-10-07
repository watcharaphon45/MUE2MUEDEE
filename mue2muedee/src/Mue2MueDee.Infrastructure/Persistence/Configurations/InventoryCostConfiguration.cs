using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class InventoryCostConfiguration : IEntityTypeConfiguration<InventoryCost>
{
    public void Configure(EntityTypeBuilder<InventoryCost> builder)
    {
        builder.ToTable("inventory_costs");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CostType)
            .IsRequired();

        builder.Property(c => c.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasIndex(c => new { c.StoreId, c.InventoryItemId });
        builder.HasIndex(c => new { c.StoreId, c.CostType });

        builder.HasOne(c => c.Store)
            .WithMany()
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.InventoryItem)
            .WithMany(i => i.Costs)
            .HasForeignKey(c => c.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
