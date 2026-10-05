using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Domain.Entities;

namespace Mue2MueDee.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Store> Stores { get; }
    DbSet<User> Users { get; }
    DbSet<UserStore> UserStores { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Brand> Brands { get; }
    DbSet<Category> Categories { get; }
    DbSet<ConditionGrade> ConditionGrades { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Product> Products { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<StockMovement> StockMovements { get; }

    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseItem> PurchaseItems { get; }
    DbSet<InspectionTemplate> InspectionTemplates { get; }
    DbSet<InspectionTemplateItem> InspectionTemplateItems { get; }
    DbSet<InspectionResult> InspectionResults { get; }
    DbSet<PurchasePayment> PurchasePayments { get; }
    DbSet<InventoryCost> InventoryCosts { get; }
    DbSet<PriceHistory> PriceHistories { get; }

    DbSet<SaleOrder> SaleOrders { get; }
    DbSet<SaleItem> SaleItems { get; }
    DbSet<SalePayment> SalePayments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
