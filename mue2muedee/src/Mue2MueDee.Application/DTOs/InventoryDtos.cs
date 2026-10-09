using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.DTOs;

public record InventoryItemDto(
    Guid Id,
    string Sku,
    Guid ProductId,
    string ProductModelName,
    string ProductCode,
    string BrandName,
    string CategoryName,
    string? ProductImageUrl,
    string SerialNumber,
    Guid ConditionGradeId,
    string ConditionGradeCode,
    string ConditionGradeName,
    InventoryStatus Status,
    string StatusName,
    decimal PurchaseCost,
    decimal ActualCost,
    decimal SellingPrice,
    decimal ExpectedProfit,
    decimal ExpectedMarginPercent,
    int? ShutterCount,
    string? Accessories,
    string? Defects,
    string? Notes,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? SoldAt,
    Guid? CustomerId,
    string? CustomerName,
    int StockAgeDays,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> ImageUrls // รูปของชิ้นนี้ (เรียงตาม SortOrder, ตัวแรก = รูปปก)
);

public record CreateInventoryItemRequest(
    Guid ProductId,
    string SerialNumber,
    Guid ConditionGradeId,
    decimal PurchaseCost,
    decimal? ActualCost,
    decimal SellingPrice,
    int? ShutterCount,
    string? Accessories,
    string? Defects,
    string? Notes,
    Guid? CustomerId,
    DateTimeOffset? ReceivedAt,
    List<string>? ImageUrls = null
);

public record UpdateInventoryItemRequest(
    string SerialNumber,
    Guid ConditionGradeId,
    decimal SellingPrice,
    int? ShutterCount,
    string? Accessories,
    string? Defects,
    string? Notes,
    List<string>? ImageUrls = null // null = ไม่แก้ไขรูป, [] = ลบรูปทั้งหมด
);

public record UpdateInventoryImagesRequest(
    List<string> ImageUrls // เรียงลำดับตามที่ต้องการ ตัวแรก = รูปปก
);

public record AdjustInventoryCostRequest(
    decimal AdditionalCost,
    string CostTypeReason, // e.g. "ค่าทำความสะอาดเซนเซอร์", "ค่าเปลี่ยนยางบวม", "ค่าขนส่ง"
    string? Notes
);

public record UpdateInventoryStatusRequest(
    InventoryStatus NewStatus,
    string? Reason,
    string? ReferenceNumber
);

public record StockMovementDto(
    Guid Id,
    Guid InventoryItemId,
    StockMovementType MovementType,
    string MovementTypeName,
    int Quantity,
    string? ReferenceNumber,
    string? Notes,
    DateTimeOffset CreatedAt,
    string? CreatedBy
);

public record InventoryQueryFilter(
    string? Search,
    Guid? BrandId,
    Guid? CategoryId,
    Guid? ProductId,
    InventoryStatus? Status,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
