using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public interface IPurchaseService
{
    Task<PagedResult<PurchaseOrderDto>> GetPurchaseOrdersPagedAsync(PurchaseQueryFilter filter);
    Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(Guid id);
    Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request);
    Task<PurchaseOrderDto> AddItemAsync(Guid poId, CreatePurchaseItemRequest request);
    Task<PurchaseOrderDto> RemoveItemAsync(Guid poId, Guid itemId);

    Task<InspectionTemplateDto> GetInspectionTemplateAsync(CategoryType categoryType);
    Task<PurchaseOrderDto> SaveInspectionResultsAsync(Guid poId, Guid itemId, SaveInspectionResultsRequest request);

    Task<PurchaseOrderDto> MakeOfferAsync(Guid poId, MakeOfferRequest request);
    Task<PurchaseOrderDto> ApprovePurchaseAsync(Guid poId);
    Task<PurchaseOrderDto> AcceptOfferAsync(Guid poId);
    Task<PurchaseOrderDto> RejectOfferAsync(Guid poId);

    Task<PurchaseOrderDto> RecordPaymentAsync(Guid poId, RecordPurchasePaymentRequest request);
    Task<PurchaseOrderDto> ReceiveToStockAsync(Guid poId);
    Task DeletePurchaseOrderAsync(Guid id);
}
