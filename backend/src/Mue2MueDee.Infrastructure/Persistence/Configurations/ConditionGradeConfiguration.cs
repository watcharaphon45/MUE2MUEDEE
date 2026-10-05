using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class ConditionGradeConfiguration : IEntityTypeConfiguration<ConditionGrade>
{
    public void Configure(EntityTypeBuilder<ConditionGrade> builder)
    {
        builder.ToTable("condition_grades");
        builder.HasKey(cg => cg.Id);

        builder.Property(cg => cg.GradeCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(cg => cg.GradeName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cg => cg.Description)
            .HasMaxLength(500);

        builder.HasIndex(cg => new { cg.StoreId, cg.GradeCode })
            .IsUnique();

        builder.HasOne(cg => cg.Store)
            .WithMany()
            .HasForeignKey(cg => cg.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
