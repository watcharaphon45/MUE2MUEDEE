using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class InventoryItemImageConfiguration : IEntityTypeConfiguration<InventoryItemImage>
{
    public void Configure(EntityTypeBuilder<InventoryItemImage> builder)
    {
        builder.ToTable("inventory_item_images");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.HasIndex(x => new { x.StoreId, x.InventoryItemId, x.SortOrder });

        builder.HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InventoryItem)
            .WithMany(i => i.Images)
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
