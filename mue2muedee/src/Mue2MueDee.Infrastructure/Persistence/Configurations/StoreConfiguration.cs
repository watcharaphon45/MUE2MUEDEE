using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("stores");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Phone)
            .HasMaxLength(50);

        builder.Property(s => s.Email)
            .HasMaxLength(150);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.TaxId)
            .HasMaxLength(50);

        builder.Property(s => s.Facebook)
            .HasMaxLength(200);

        builder.Property(s => s.Line)
            .HasMaxLength(100);

        builder.Property(s => s.Website)
            .HasMaxLength(200);

        builder.Property(s => s.PrimaryColor)
            .HasMaxLength(20)
            .HasDefaultValue("#4f46e5");

        builder.HasIndex(s => s.Code).IsUnique();

        // Soft Delete Global Query Filter
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
