using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.DTOs;

// --- Purchase Order DTOs ---

public record PurchaseOrderDto(
    Guid Id,
    string PoNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    string? CustomerIdCardNumber,
    PurchaseStatus Status,
    string StatusName,
    decimal TotalEstimatedValue,
    decimal TotalMaxBuyPrice,
    decimal TotalOfferedPrice,
    decimal TotalAgreedPrice,
    bool NeedsApproval,
    string? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    string? Notes,
    DateTimeOffset PurchaseDate,
    IReadOnlyList<PurchaseItemDto> Items,
    IReadOnlyList<PurchasePaymentDto> Payments,
    DateTimeOffset CreatedAt
);

public record PurchaseItemDto(
    Guid Id,
    Guid PurchaseOrderId,
    Guid ProductId,
    string ProductModelName,
    string ProductCode,
    string BrandName,
    string CategoryName,
    CategoryType CategoryType,
    string SerialNumber,
    Guid ConditionGradeId,
    string ConditionGradeCode,
    string ConditionGradeName,
    int? ShutterCount,
    string? Accessories,
    string? Defects,
    decimal ExpectedSalePrice,
    decimal EstimatedRepairCost,
    decimal EstimatedCleaningCost,
    decimal MissingAccessoriesDeduction,
    decimal MaxBuyPrice,
    decimal OfferedPrice,
    decimal AgreedPrice,
    Guid? InventoryItemId,
    string? GeneratedSku,
    IReadOnlyList<InspectionResultDto> InspectionResults
);

public record InspectionResultDto(
    Guid Id,
    Guid InspectionTemplateItemId,
    string CategoryGroup,
    string Title,
    bool IsCritical,
    InspectionItemResult Result,
    string ResultName,
    string? Notes,
    string? PhotoUrl
);

public record PurchasePaymentDto(
    Guid Id,
    PaymentMethod PaymentMethod,
    string PaymentMethodName,
    decimal Amount,
    DateTimeOffset PaidAt,
    string? ReferenceNumber,
    string? ProofUrl,
    string? Notes
);

// --- Inspection Template DTOs ---

public record InspectionTemplateDto(
    Guid Id,
    CategoryType CategoryType,
    string CategoryTypeName,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<InspectionTemplateItemDto> Items
);

public record InspectionTemplateItemDto(
    Guid Id,
    string CategoryGroup,
    string Title,
    string? Description,
    int SortOrder,
    bool IsCritical
);

// --- Requests ---

public record CreatePurchaseOrderRequest(
    Guid CustomerId,
    string? Notes,
    List<CreatePurchaseItemRequest>? InitialItems
);

public record CreatePurchaseItemRequest(
    Guid ProductId,
    string SerialNumber,
    Guid ConditionGradeId,
    int? ShutterCount,
    string? Accessories,
    string? Defects,
    decimal ExpectedSalePrice,
    decimal EstimatedRepairCost = 0,
    decimal EstimatedCleaningCost = 0,
    decimal MissingAccessoriesDeduction = 0,
    decimal? OfferedPrice = null
);

public record SaveInspectionItemRequest(
    Guid TemplateItemId,
    InspectionItemResult Result,
    string? Notes,
    string? PhotoUrl
);

public record SaveInspectionResultsRequest(
    List<SaveInspectionItemRequest> Results
);

public record MakeOfferRequest(
    decimal TotalOfferedPrice,
    string? Notes
);

public record RecordPurchasePaymentRequest(
    PaymentMethod PaymentMethod,
    decimal Amount,
    string? ReferenceNumber,
    string? ProofUrl,
    string? Notes
);

public record PurchaseQueryFilter(
    string? Search,
    PurchaseStatus? Status,
    Guid? CustomerId,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int Page = 1,
    int PageSize = 20
);
