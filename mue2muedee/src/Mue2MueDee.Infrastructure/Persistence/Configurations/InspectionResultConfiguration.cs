using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class InspectionResultConfiguration : IEntityTypeConfiguration<InspectionResult>
{
    public void Configure(EntityTypeBuilder<InspectionResult> builder)
    {
        builder.ToTable("inspection_results");
        builder.HasKey(ir => ir.Id);

        builder.Property(ir => ir.Notes).HasMaxLength(500);
        builder.Property(ir => ir.PhotoUrl).HasMaxLength(1000);

        builder.HasIndex(ir => new { ir.PurchaseItemId, ir.InspectionTemplateItemId });

        builder.HasOne(ir => ir.InspectionTemplateItem)
            .WithMany()
            .HasForeignKey(ir => ir.InspectionTemplateItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
