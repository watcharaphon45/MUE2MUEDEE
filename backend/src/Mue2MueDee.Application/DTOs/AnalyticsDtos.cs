using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.DTOs;

public record DashboardMetricsDto(
    int AvailableItems,
    int PendingInspection,
    int RepairCount,
    int ReservedCount,
    int TotalActiveStockCount,
    int SoldThisMonthCount,
    decimal SalesTodayAmount,
    int SalesTodayCount,
    decimal SalesThisMonthAmount,
    int SalesThisMonthCount,
    decimal AverageOrderValue,
    int StockAgingOver60DaysCount,
    int StockAgingOver90DaysCount,
    int StockAgingOver120DaysCount,
    // Financial metrics (Visible only to Owner/Admin)
    bool IsFinancialVisible,
    decimal? InventoryCostValue,
    decimal? InventorySalesValue,
    decimal? GrossProfitThisMonth,
    decimal? AverageProfitMarginThisMonth,
    decimal? DeadCapitalCost,
    IReadOnlyList<RecentSaleDto> RecentSales,
    IReadOnlyList<RecentPurchaseDto> RecentPurchases
);

public record RecentSaleDto(
    Guid OrderId,
    string OrderNumber,
    string CustomerName,
    int ItemCount,
    decimal NetAmount,
    decimal? GrossProfit,
    DateTimeOffset OrderDate,
    SaleStatus Status
);

public record RecentPurchaseDto(
    Guid PurchaseOrderId,
    string PoNumber,
    string CustomerName,
    int ItemCount,
    decimal TotalAgreedPrice,
    DateTimeOffset PoDate,
    PurchaseStatus Status
);

public record AgingBucketSummaryDto(
    string BucketKey,
    string Label,
    string StatusLevel, // "fresh", "normal", "warning", "danger", "critical"
    string ColorCode,
    int Count,
    decimal TotalCost,
    decimal TotalSalesValue,
    decimal DiscountPercentage
);

public record AgingItemDetailDto(
    Guid InventoryItemId,
    string Sku,
    string? SerialNumber,
    string BrandName,
    string CategoryName,
    string ModelName,
    int DaysInStock,
    DateTimeOffset ReceivedAt,
    string ConditionGrade,
    decimal ActualCost,
    decimal CurrentSellingPrice,
    decimal RecommendedClearancePrice,
    string AgingBucket,
    decimal AgingDiscountPercentage,
    InventoryStatus Status
);

public record StockAgingReportDto(
    int TotalItemsInStock,
    bool IsFinancialVisible,
    decimal? TotalCostValue,
    decimal TotalSalesValue,
    decimal? DeadCapitalCostValue,
    IReadOnlyList<AgingBucketSummaryDto> Buckets,
    IReadOnlyList<AgingItemDetailDto> AgingItems
);

public record SalesReportQuery(
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    Guid? BrandId = null,
    Guid? CategoryId = null,
    PaymentMethod? PaymentMethod = null
);

public record SalesReportSummaryDto(
    int OrderCount,
    int ItemsSoldCount,
    decimal TotalGrossSales,
    decimal TotalDiscount,
    decimal TotalNetSales,
    bool IsFinancialVisible,
    decimal? TotalCost,
    decimal? TotalGrossProfit,
    decimal? AverageMarginPercentage
);

public record ReportGroupSummaryDto(
    Guid GroupId,
    string GroupName,
    int QuantitySold,
    decimal TotalSales,
    decimal? TotalCost,
    decimal? GrossProfit,
    decimal? MarginPercentage
);

public record PaymentMethodBreakdownDto(
    PaymentMethod Method,
    string MethodName,
    int TransactionCount,
    decimal TotalAmount,
    decimal Percentage
);

public record DailySalesTrendDto(
    string DateString,
    DateTime Date,
    int OrderCount,
    decimal NetSales,
    decimal? GrossProfit
);

public record SalesReportOrderDto(
    Guid OrderId,
    string OrderNumber,
    DateTimeOffset OrderDate,
    string CustomerName,
    int ItemCount,
    string PaymentMethods,
    decimal NetAmount,
    decimal? GrossProfit,
    decimal? MarginPercentage,
    SaleStatus Status
);

public record SalesReportDto(
    SalesReportSummaryDto Summary,
    IReadOnlyList<ReportGroupSummaryDto> BrandBreakdown,
    IReadOnlyList<ReportGroupSummaryDto> CategoryBreakdown,
    IReadOnlyList<PaymentMethodBreakdownDto> PaymentBreakdown,
    IReadOnlyList<DailySalesTrendDto> DailyTrend,
    IReadOnlyList<SalesReportOrderDto> Orders
);
