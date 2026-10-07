using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");
        builder.HasKey(po => po.Id);

        builder.Property(po => po.PoNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(po => po.TotalEstimatedValue).HasPrecision(18, 2);
        builder.Property(po => po.TotalMaxBuyPrice).HasPrecision(18, 2);
        builder.Property(po => po.TotalOfferedPrice).HasPrecision(18, 2);
        builder.Property(po => po.TotalAgreedPrice).HasPrecision(18, 2);

        builder.Property(po => po.ApprovedBy).HasMaxLength(100);
        builder.Property(po => po.Notes).HasMaxLength(1000);

        builder.HasIndex(po => new { po.StoreId, po.PoNumber }).IsUnique();
        builder.HasIndex(po => new { po.StoreId, po.Status });
        builder.HasIndex(po => new { po.StoreId, po.CustomerId });

        builder.HasOne(po => po.Store)
            .WithMany()
            .HasForeignKey(po => po.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.Customer)
            .WithMany()
            .HasForeignKey(po => po.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Items)
            .WithOne(pi => pi.PurchaseOrder)
            .HasForeignKey(pi => pi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(po => po.Payments)
            .WithOne(pp => pp.PurchaseOrder)
            .HasForeignKey(pp => pp.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
