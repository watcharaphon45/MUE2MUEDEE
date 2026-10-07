using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class SalePaymentConfiguration : IEntityTypeConfiguration<SalePayment>
{
    public void Configure(EntityTypeBuilder<SalePayment> builder)
    {
        builder.ToTable("sale_payments");
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sp => sp.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(sp => sp.Notes)
            .HasMaxLength(500);

        builder.Property(sp => sp.CreatedBy)
            .HasMaxLength(100);

        builder.HasIndex(sp => new { sp.StoreId, sp.SaleOrderId });
        builder.HasIndex(sp => sp.PaidAt);

        builder.HasOne(sp => sp.Store)
            .WithMany()
            .HasForeignKey(sp => sp.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.SaleOrder)
            .WithMany(so => so.Payments)
            .HasForeignKey(sp => sp.SaleOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
