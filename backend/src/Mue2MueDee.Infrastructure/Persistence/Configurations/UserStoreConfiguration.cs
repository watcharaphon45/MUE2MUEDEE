using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence.Configurations;

public class UserStoreConfiguration : IEntityTypeConfiguration<UserStore>
{
    public void Configure(EntityTypeBuilder<UserStore> builder)
    {
        builder.ToTable("user_stores");
        builder.HasKey(us => us.Id);

        builder.HasIndex(us => new { us.UserId, us.StoreId }).IsUnique();

        builder.HasOne(us => us.User)
            .WithMany(u => u.UserStores)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(us => us.Store)
            .WithMany(s => s.UserStores)
            .HasForeignKey(us => us.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft Delete Matching Query Filter
        builder.HasQueryFilter(us => !us.Store.IsDeleted && !us.User.IsDeleted);
    }
}
