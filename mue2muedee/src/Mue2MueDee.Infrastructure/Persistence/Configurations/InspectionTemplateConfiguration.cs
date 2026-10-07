using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class InspectionTemplateConfiguration : IEntityTypeConfiguration<InspectionTemplate>
{
    public void Configure(EntityTypeBuilder<InspectionTemplate> builder)
    {
        builder.ToTable("inspection_templates");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.HasIndex(t => new { t.StoreId, t.CategoryType });

        builder.HasOne(t => t.Store)
            .WithMany()
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Items)
            .WithOne(ti => ti.Template)
            .HasForeignKey(ti => ti.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
