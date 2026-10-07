using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class PurchasePaymentConfiguration : IEntityTypeConfiguration<PurchasePayment>
{
    public void Configure(EntityTypeBuilder<PurchasePayment> builder)
    {
        builder.ToTable("purchase_payments");
        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.Amount).HasPrecision(18, 2);
        builder.Property(pp => pp.ReferenceNumber).HasMaxLength(100);
        builder.Property(pp => pp.ProofUrl).HasMaxLength(1000);
        builder.Property(pp => pp.Notes).HasMaxLength(500);

        builder.HasIndex(pp => new { pp.StoreId, pp.PurchaseOrderId });

        builder.HasOne(pp => pp.Store)
            .WithMany()
            .HasForeignKey(pp => pp.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
