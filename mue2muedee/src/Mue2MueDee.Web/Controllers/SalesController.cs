using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

public record CancelSaleRequest(string Reason);
public record ReserveItemRequest(string? CustomerName, string? Notes);
public record ReleaseReservationRequest(string? Reason);

[Authorize]
[Route("api/sales")]
public class SalesController : ApiControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSales([FromQuery] SaleQueryFilter filter)
    {
        var result = await _saleService.GetSalesPagedAsync(filter);
        return Success(result, "ดึงรายการขายสำเร็จ");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSaleById(Guid id)
    {
        var result = await _saleService.GetSaleByIdAsync(id);
        return Success(result, "ดึงข้อมูลรายการขายสำเร็จ");
    }

    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleOrderRequest request)
    {
        var result = await _saleService.CreateSaleAsync(request);
        return CreatedSuccess(result, $"บันทึกการขายสำเร็จ เลขที่ {result.SaleNumber}");
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelSale(Guid id, [FromBody] CancelSaleRequest request)
    {
        var result = await _saleService.CancelSaleAsync(id, request.Reason);
        return Success(result, "ยกเลิกรายการขายและนำสินค้ากลับเข้าคลังสำเร็จ");
    }

    [HttpGet("trace/{serialNumber}")]
    public async Task<IActionResult> GetSerialTrace(string serialNumber)
    {
        var result = await _saleService.GetSerialTraceAsync(serialNumber);
        return Success(result, "ดึงประวัติสินค้าตาม Serial Number สำเร็จ");
    }

    [HttpPost("reserve/{inventoryItemId:guid}")]
    public async Task<IActionResult> ReserveItem(Guid inventoryItemId, [FromBody] ReserveItemRequest request)
    {
        await _saleService.ReserveItemAsync(inventoryItemId, request.CustomerName, request.Notes);
        return Success<object?>(null, "จองสินค้าสำเร็จ");
    }

    [HttpPost("release-reservation/{inventoryItemId:guid}")]
    public async Task<IActionResult> ReleaseReservation(Guid inventoryItemId, [FromBody] ReleaseReservationRequest request)
    {
        await _saleService.ReleaseReservationAsync(inventoryItemId, request.Reason);
        return Success<object?>(null, "ยกเลิกการจองสินค้าสำเร็จ");
    }
}
