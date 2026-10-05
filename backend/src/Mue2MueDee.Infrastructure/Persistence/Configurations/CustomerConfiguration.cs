using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Phone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.Email)
            .HasMaxLength(150);

        builder.Property(c => c.IdCardNumber)
            .HasMaxLength(30);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.LineId)
            .HasMaxLength(100);

        builder.Property(c => c.Facebook)
            .HasMaxLength(100);

        builder.Property(c => c.BankName)
            .HasMaxLength(100);

        builder.Property(c => c.BankAccountNumber)
            .HasMaxLength(50);

        builder.Property(c => c.BankAccountName)
            .HasMaxLength(150);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(c => new { c.StoreId, c.Phone });
        builder.HasIndex(c => new { c.StoreId, c.IdCardNumber });

        builder.HasOne(c => c.Store)
            .WithMany()
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
