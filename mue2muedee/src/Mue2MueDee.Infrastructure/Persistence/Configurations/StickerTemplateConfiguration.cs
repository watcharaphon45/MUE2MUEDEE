using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class StickerTemplateConfiguration : IEntityTypeConfiguration<StickerTemplate>
{
    public void Configure(EntityTypeBuilder<StickerTemplate> builder)
    {
        builder.ToTable("sticker_templates");
        builder.HasKey(st => st.Id);

        builder.Property(st => st.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(st => st.Description)
            .HasMaxLength(500);

        builder.Property(st => st.PaperWidthMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.PaperHeightMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.LabelWidthMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.LabelHeightMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.HorizontalGapMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.VerticalGapMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.MarginTopMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.MarginBottomMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.MarginLeftMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.MarginRightMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.StoreLogoUrl)
            .HasMaxLength(500);

        builder.Property(st => st.StoreName)
            .HasMaxLength(150);

        builder.Property(st => st.FooterText)
            .HasMaxLength(150);

        builder.Property(st => st.CodeStyle)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(st => st.EncodeValue)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(st => st.BarcodeHeightMm)
            .HasPrecision(8, 2);

        builder.Property(st => st.BarcodeWidthRatio)
            .HasPrecision(8, 2);

        builder.Property(st => st.QrSizeMm)
            .HasPrecision(8, 2);

        builder.HasIndex(st => new { st.StoreId, st.Name });
        builder.HasIndex(st => new { st.StoreId, st.IsDefault });

        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(st => st.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
