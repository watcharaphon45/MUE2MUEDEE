using Mue2MueDee.Application.DTOs;

namespace Mue2MueDee.Application.Services;

public interface ISaleService
{
    Task<PagedResult<SaleOrderDto>> GetSalesPagedAsync(SaleQueryFilter filter);
    Task<SaleOrderDto> GetSaleByIdAsync(Guid id);
    Task<SaleOrderDto> CreateSaleAsync(CreateSaleOrderRequest request);
    Task<SaleOrderDto> CancelSaleAsync(Guid id, string reason);
    Task<SerialTraceDto> GetSerialTraceAsync(string serialNumber);
    Task ReserveItemAsync(Guid inventoryItemId, string? customerName, string? notes);
    Task ReleaseReservationAsync(Guid inventoryItemId, string? reason);
}
