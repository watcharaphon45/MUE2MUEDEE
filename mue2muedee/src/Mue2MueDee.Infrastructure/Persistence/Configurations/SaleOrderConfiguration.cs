using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class SaleOrderConfiguration : IEntityTypeConfiguration<SaleOrder>
{
    public void Configure(EntityTypeBuilder<SaleOrder> builder)
    {
        builder.ToTable("sale_orders");
        builder.HasKey(so => so.Id);

        builder.Property(so => so.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(so => so.CustomerName)
            .HasMaxLength(200);

        builder.Property(so => so.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(so => so.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(so => so.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(so => so.NetAmount)
            .HasPrecision(18, 2);

        builder.Property(so => so.TotalCost)
            .HasPrecision(18, 2);

        builder.Property(so => so.TotalGrossProfit)
            .HasPrecision(18, 2);

        builder.Property(so => so.TotalMarginPercentage)
            .HasPrecision(18, 2);

        builder.Property(so => so.Notes)
            .HasMaxLength(1000);

        builder.Property(so => so.CreatedBy)
            .HasMaxLength(100);

        builder.HasIndex(so => new { so.StoreId, so.SaleNumber })
            .IsUnique();

        builder.HasIndex(so => new { so.StoreId, so.Status });
        builder.HasIndex(so => new { so.StoreId, so.SaleDate });

        builder.HasOne(so => so.Store)
            .WithMany()
            .HasForeignKey(so => so.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(so => so.Customer)
            .WithMany()
            .HasForeignKey(so => so.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
