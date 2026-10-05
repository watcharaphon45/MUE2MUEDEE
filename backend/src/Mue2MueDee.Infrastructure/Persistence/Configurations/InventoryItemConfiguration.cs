using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.PurchaseCost)
            .HasPrecision(18, 2);

        builder.Property(i => i.ActualCost)
            .HasPrecision(18, 2);

        builder.Property(i => i.SellingPrice)
            .HasPrecision(18, 2);

        builder.Property(i => i.Accessories)
            .HasMaxLength(1000);

        builder.Property(i => i.Defects)
            .HasMaxLength(1000);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.Property(i => i.RowVersion)
            .IsRowVersion();

        // Unique SKU per store
        builder.HasIndex(i => new { i.StoreId, i.Sku })
            .IsUnique();

        // Unique Serial Number per product per store
        builder.HasIndex(i => new { i.StoreId, i.ProductId, i.SerialNumber })
            .IsUnique();

        // Indexes for fast searching and filtering
        builder.HasIndex(i => new { i.StoreId, i.Status });
        builder.HasIndex(i => new { i.StoreId, i.SerialNumber });

        builder.HasOne(i => i.Store)
            .WithMany()
            .HasForeignKey(i => i.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Product)
            .WithMany(p => p.InventoryItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ConditionGrade)
            .WithMany(cg => cg.InventoryItems)
            .HasForeignKey(i => i.ConditionGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Customer)
            .WithMany(c => c.SourcedInventoryItems)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
