using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");
        builder.HasKey(si => si.Id);

        builder.Property(si => si.ProductModelName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(si => si.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(si => si.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(si => si.ConditionGradeCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(si => si.ActualCost)
            .HasPrecision(18, 2);

        builder.Property(si => si.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(si => si.Discount)
            .HasPrecision(18, 2);

        builder.Property(si => si.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(si => si.GrossProfit)
            .HasPrecision(18, 2);

        builder.Property(si => si.MarginPercentage)
            .HasPrecision(18, 2);

        builder.HasIndex(si => new { si.StoreId, si.SaleOrderId });
        builder.HasIndex(si => new { si.StoreId, si.InventoryItemId });
        builder.HasIndex(si => new { si.StoreId, si.SerialNumber });

        builder.HasOne(si => si.Store)
            .WithMany()
            .HasForeignKey(si => si.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.SaleOrder)
            .WithMany(so => so.Items)
            .HasForeignKey(si => si.SaleOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.InventoryItem)
            .WithMany()
            .HasForeignKey(si => si.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.Product)
            .WithMany()
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
