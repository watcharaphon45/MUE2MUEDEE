using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentStore _currentStore;
    private readonly ICurrentUser _currentUser;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentStore currentStore,
        ICurrentUser currentUser) : base(options)
    {
        _currentStore = currentStore;
        _currentUser = currentUser;
    }

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserStore> UserStores => Set<UserStore>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ConditionGrade> ConditionGrades => Set<ConditionGrade>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<InspectionTemplate> InspectionTemplates => Set<InspectionTemplate>();
    public DbSet<InspectionTemplateItem> InspectionTemplateItems => Set<InspectionTemplateItem>();
    public DbSet<InspectionResult> InspectionResults => Set<InspectionResult>();
    public DbSet<PurchasePayment> PurchasePayments => Set<PurchasePayment>();
    public DbSet<InventoryCost> InventoryCosts => Set<InventoryCost>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<SaleOrder> SaleOrders => Set<SaleOrder>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations automatically from the Configurations/ folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply Global Query Filters for ITenantEntity and ISoftDelete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var isTenant = typeof(ITenantEntity).IsAssignableFrom(clrType);
            var isSoftDelete = typeof(ISoftDelete).IsAssignableFrom(clrType);

            // UserStore has its own custom navigation query filter in UserStoreConfiguration
            if (clrType == typeof(UserStore)) continue;

            if (isTenant || isSoftDelete)
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(ConfigureGlobalFilters), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .MakeGenericMethod(clrType);
                method?.Invoke(this, [modelBuilder, isTenant, isSoftDelete]);
            }
        }
    }

    private void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder, bool isTenant, bool isSoftDelete) where TEntity : class
    {
        if (isTenant && isSoftDelete)
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
                !EF.Property<bool>(e, "IsDeleted") &&
                (_currentStore.StoreId == null || EF.Property<Guid>(e, "StoreId") == _currentStore.StoreId));
        }
        else if (isTenant)
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
                _currentStore.StoreId == null || EF.Property<Guid>(e, "StoreId") == _currentStore.StoreId);
        }
        else if (isSoftDelete)
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => !EF.Property<bool>(e, "IsDeleted"));
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var currentUsername = _currentUser.Username ?? "system";
        var currentStoreId = _currentStore.StoreId;

        var auditEntries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries())
        {
            // Auto-assign / protect Tenant StoreId
            if (entry.Entity is ITenantEntity tenantEntity)
            {
                if (entry.State == EntityState.Added && tenantEntity.StoreId == Guid.Empty && currentStoreId.HasValue)
                {
                    tenantEntity.StoreId = currentStoreId.Value;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var storeIdProp = entry.Property(nameof(ITenantEntity.StoreId));
                    if (storeIdProp.IsModified)
                    {
                        throw new InvalidOperationException("ไม่อนุญาตให้แก้ไข StoreId ของข้อมูลเดิม");
                    }
                }
            }

            // Handle AuditableEntity timestamps
            if (entry.Entity is AuditableEntity<Guid> auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = now;
                    auditable.CreatedBy ??= currentUsername;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAt = now;
                    auditable.UpdatedBy = currentUsername;
                }
            }

            // Handle Soft Delete
            if (entry.Entity is ISoftDelete softDelete && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDelete.IsDeleted = true;
                softDelete.DeletedAt = now;
                softDelete.DeletedBy = currentUsername;
            }

            // Create Audit Log for non-audit entities
            if (entry.Entity is not AuditLog && entry.Entity is not RefreshToken &&
                (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted))
            {
                var audit = new AuditLog
                {
                    StoreId = currentStoreId,
                    UserId = _currentUser.UserId,
                    UserName = currentUsername,
                    Action = entry.State.ToString().ToUpperInvariant(),
                    EntityName = entry.Entity.GetType().Name,
                    CreatedAt = now
                };

                var entityKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                audit.EntityId = entityKey?.CurrentValue?.ToString() ?? "Unknown";

                if (entry.State == EntityState.Added)
                {
                    var newValues = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                    audit.NewValues = JsonSerializer.Serialize(newValues);
                }
                else if (entry.State == EntityState.Modified)
                {
                    var oldValues = entry.Properties.Where(p => p.IsModified).ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                    var newValues = entry.Properties.Where(p => p.IsModified).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                    audit.OldValues = JsonSerializer.Serialize(oldValues);
                    audit.NewValues = JsonSerializer.Serialize(newValues);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    var oldValues = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                    audit.OldValues = JsonSerializer.Serialize(oldValues);
                }

                auditEntries.Add(audit);
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
