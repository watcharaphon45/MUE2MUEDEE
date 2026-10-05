using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Models;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[Authorize]
[Route("api/inventory")]
public class InventoryController : ApiControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventory([FromQuery] InventoryQueryFilter filter)
    {
        var result = await _inventoryService.GetInventoryPagedAsync(filter);
        return Success(result, "ดึงรายการสินค้าในคลังสำเร็จ");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInventoryById(Guid id)
    {
        var item = await _inventoryService.GetInventoryItemByIdAsync(id);
        return Success(item, "ดึงข้อมูลสินค้าสำเร็จ");
    }

    [HttpGet("lookup/{code}")]
    public async Task<IActionResult> LookupBySkuOrSerial(string code)
    {
        var item = await _inventoryService.GetInventoryItemBySkuOrSerialAsync(code);
        return Success(item, "ค้นหาข้อมูลสินค้าสำเร็จ");
    }

    [HttpPost]
    public async Task<IActionResult> CreateInventoryItem([FromBody] CreateInventoryItemRequest request)
    {
        var item = await _inventoryService.CreateInventoryItemAsync(request);
        return CreatedSuccess(item, $"รับสินค้า {item.ProductModelName} (SN: {item.SerialNumber}) เข้าคลังสำเร็จ (SKU: {item.Sku})");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateInventoryItem(Guid id, [FromBody] UpdateInventoryItemRequest request)
    {
        var item = await _inventoryService.UpdateInventoryItemAsync(id, request);
        return Success(item, $"แก้ไขข้อมูลสินค้า {item.ProductModelName} (SKU: {item.Sku}) สำเร็จ");
    }

    [HttpPost("{id:guid}/adjust-cost")]
    public async Task<IActionResult> AdjustCost(Guid id, [FromBody] AdjustInventoryCostRequest request)
    {
        var item = await _inventoryService.AdjustCostAsync(id, request);
        return Success(item, $"บันทึกต้นทุนเพิ่มเติมสำเร็จ ต้นทุนจริงปัจจุบันคือ ฿{item.ActualCost:N2}");
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateInventoryStatusRequest request)
    {
        var item = await _inventoryService.UpdateStatusAsync(id, request);
        return Success(item, $"เปลี่ยนสถานะสินค้าเป็น '{item.StatusName}' สำเร็จ");
    }

    [HttpGet("{id:guid}/movements")]
    public async Task<IActionResult> GetMovements(Guid id)
    {
        var movements = await _inventoryService.GetStockMovementsAsync(id);
        return Success(movements, "ดึงประวัติการเคลื่อนไหวสต็อกสำเร็จ");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteInventoryItem(Guid id)
    {
        await _inventoryService.DeleteInventoryItemAsync(id);
        return Success("ลบสินค้าออกจากคลังสำเร็จ");
    }
}
