using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.DTOs;

public record SaleItemDto(
    Guid Id,
    Guid SaleOrderId,
    Guid InventoryItemId,
    Guid ProductId,
    string ProductModelName,
    string Sku,
    string SerialNumber,
    string ConditionGradeCode,
    decimal ActualCost,
    decimal UnitPrice,
    decimal Discount,
    decimal SubTotal,
    decimal GrossProfit,
    decimal MarginPercentage,
    int WarrantyDays
);

public record SalePaymentDto(
    Guid Id,
    Guid SaleOrderId,
    PaymentMethod PaymentMethod,
    string PaymentMethodName,
    decimal Amount,
    string? ReferenceNumber,
    DateTimeOffset PaidAt,
    string? Notes,
    string CreatedBy
);

public record SaleOrderDto(
    Guid Id,
    string SaleNumber,
    Guid? CustomerId,
    string? CustomerName,
    string? CustomerPhone,
    DateTimeOffset SaleDate,
    SaleStatus Status,
    string StatusName,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal NetAmount,
    decimal TotalCost,
    decimal TotalGrossProfit,
    decimal TotalMarginPercentage,
    string? Notes,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    IReadOnlyList<SaleItemDto> Items,
    IReadOnlyList<SalePaymentDto> Payments
);

public record CreateSaleItemRequest(
    Guid InventoryItemId,
    decimal UnitPrice,
    decimal? Discount,
    int? WarrantyDays
);

public record CreateSalePaymentRequest(
    PaymentMethod PaymentMethod,
    decimal Amount,
    string? ReferenceNumber,
    string? Notes
);

public record CreateSaleOrderRequest(
    Guid? CustomerId,
    string? CustomerName,
    string? CustomerPhone,
    string? Notes,
    List<CreateSaleItemRequest> Items,
    List<CreateSalePaymentRequest> Payments
);

public record SaleQueryFilter(
    string? Search = null,
    SaleStatus? Status = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    int Page = 1,
    int PageSize = 20
);

public record SerialTraceDto(
    string SerialNumber,
    Guid InventoryItemId,
    string Sku,
    string ProductModelName,
    string BrandName,
    string CategoryName,
    string ConditionGradeCode,
    string ConditionGradeName,
    string CurrentStatusName,
    string? SourcedFromCustomer,
    string? PurchasePoNumber,
    DateTimeOffset? ReceivedAt,
    decimal PurchaseCost,
    decimal ActualCost,
    decimal CurrentSellingPrice,
    string? SoldToCustomer,
    string? SaleNumber,
    DateTimeOffset? SoldAt,
    decimal? SoldPrice,
    decimal? GrossProfit,
    decimal? MarginPercentage,
    IReadOnlyList<StockMovementDto> Movements
);
