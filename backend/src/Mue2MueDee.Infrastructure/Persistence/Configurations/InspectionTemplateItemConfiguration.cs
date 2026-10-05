using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class InspectionTemplateItemConfiguration : IEntityTypeConfiguration<InspectionTemplateItem>
{
    public void Configure(EntityTypeBuilder<InspectionTemplateItem> builder)
    {
        builder.ToTable("inspection_template_items");
        builder.HasKey(ti => ti.Id);

        builder.Property(ti => ti.CategoryGroup)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ti => ti.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ti => ti.Description)
            .HasMaxLength(500);

        builder.HasIndex(ti => new { ti.TemplateId, ti.SortOrder });
    }
}
