using Mue2MueDee.Application.DTOs;

namespace Mue2MueDee.Application.Services;

public interface IInventoryService
{
    Task<PagedResult<InventoryItemDto>> GetInventoryPagedAsync(InventoryQueryFilter filter);
    Task<InventoryItemDto> GetInventoryItemByIdAsync(Guid id);
    Task<InventoryItemDto> GetInventoryItemBySkuOrSerialAsync(string code);
    Task<InventoryItemDto> CreateInventoryItemAsync(CreateInventoryItemRequest request);
    Task<InventoryItemDto> UpdateInventoryItemAsync(Guid id, UpdateInventoryItemRequest request);
    Task<InventoryItemDto> UpdateImagesAsync(Guid id, UpdateInventoryImagesRequest request);
    Task<InventoryItemDto> AdjustCostAsync(Guid id, AdjustInventoryCostRequest request);
    Task<InventoryItemDto> UpdateStatusAsync(Guid id, UpdateInventoryStatusRequest request);
    Task<IReadOnlyList<StockMovementDto>> GetStockMovementsAsync(Guid inventoryItemId);
    Task DeleteInventoryItemAsync(Guid id);
}
