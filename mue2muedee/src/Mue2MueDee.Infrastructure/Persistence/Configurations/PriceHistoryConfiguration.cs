using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.ToTable("price_histories");
        builder.HasKey(ph => ph.Id);

        builder.Property(ph => ph.OldPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ph => ph.NewPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ph => ph.Reason)
            .HasMaxLength(500);

        builder.Property(ph => ph.ChangedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(ph => new { ph.StoreId, ph.InventoryItemId });
        builder.HasIndex(ph => ph.ChangedAt);

        builder.HasOne(ph => ph.Store)
            .WithMany()
            .HasForeignKey(ph => ph.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ph => ph.InventoryItem)
            .WithMany(i => i.PriceHistories)
            .HasForeignKey(ph => ph.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
