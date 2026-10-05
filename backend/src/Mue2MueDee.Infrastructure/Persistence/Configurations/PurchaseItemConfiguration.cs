using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.ToTable("purchase_items");
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pi => pi.Accessories).HasMaxLength(1000);
        builder.Property(pi => pi.Defects).HasMaxLength(1000);

        builder.Property(pi => pi.ExpectedSalePrice).HasPrecision(18, 2);
        builder.Property(pi => pi.EstimatedRepairCost).HasPrecision(18, 2);
        builder.Property(pi => pi.EstimatedCleaningCost).HasPrecision(18, 2);
        builder.Property(pi => pi.MissingAccessoriesDeduction).HasPrecision(18, 2);
        builder.Property(pi => pi.MaxBuyPrice).HasPrecision(18, 2);
        builder.Property(pi => pi.OfferedPrice).HasPrecision(18, 2);
        builder.Property(pi => pi.AgreedPrice).HasPrecision(18, 2);

        builder.HasIndex(pi => new { pi.StoreId, pi.PurchaseOrderId });
        builder.HasIndex(pi => new { pi.StoreId, pi.ProductId, pi.SerialNumber });

        builder.HasOne(pi => pi.Store)
            .WithMany()
            .HasForeignKey(pi => pi.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pi => pi.Product)
            .WithMany()
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pi => pi.ConditionGrade)
            .WithMany()
            .HasForeignKey(pi => pi.ConditionGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pi => pi.InventoryItem)
            .WithMany()
            .HasForeignKey(pi => pi.InventoryItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(pi => pi.InspectionResults)
            .WithOne(ir => ir.PurchaseItem)
            .HasForeignKey(ir => ir.PurchaseItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
