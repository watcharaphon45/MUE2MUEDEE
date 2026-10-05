using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[Authorize]
[Route("api/purchases")]
public class PurchasesController : ApiControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchasesController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPurchases([FromQuery] PurchaseQueryFilter filter)
    {
        var result = await _purchaseService.GetPurchaseOrdersPagedAsync(filter);
        return Success(result, "ดึงรายการใบรับซื้อสำเร็จ");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPurchaseById(Guid id)
    {
        var po = await _purchaseService.GetPurchaseOrderByIdAsync(id);
        return Success(po, "ดึงข้อมูลใบรับซื้อสำเร็จ");
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseOrderRequest request)
    {
        var po = await _purchaseService.CreatePurchaseOrderAsync(request);
        return CreatedSuccess(po, $"สร้างใบรับซื้อเลขที่ {po.PoNumber} สำเร็จ");
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] CreatePurchaseItemRequest request)
    {
        var po = await _purchaseService.AddItemAsync(id, request);
        return Success(po, "เพิ่มรายการสินค้าในใบรับซื้อสำเร็จ");
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        var po = await _purchaseService.RemoveItemAsync(id, itemId);
        return Success(po, "ลบรายการสินค้าออกจากใบรับซื้อสำเร็จ");
    }

    [HttpGet("inspection-template/{categoryType}")]
    public async Task<IActionResult> GetInspectionTemplate(CategoryType categoryType)
    {
        var template = await _purchaseService.GetInspectionTemplateAsync(categoryType);
        return Success(template, "ดึงแบบฟอร์มเกณฑ์ตรวจสภาพสำเร็จ");
    }

    [HttpPost("{id:guid}/items/{itemId:guid}/inspection")]
    public async Task<IActionResult> SaveInspection(Guid id, Guid itemId, [FromBody] SaveInspectionResultsRequest request)
    {
        var po = await _purchaseService.SaveInspectionResultsAsync(id, itemId, request);
        return Success(po, "บันทึกผลการตรวจเช็คสภาพสินค้าสำเร็จ");
    }

    [HttpPost("{id:guid}/offer")]
    public async Task<IActionResult> MakeOffer(Guid id, [FromBody] MakeOfferRequest request)
    {
        var po = await _purchaseService.MakeOfferAsync(id, request);
        var msg = po.NeedsApproval
            ? $"เสนอราคา ฿{po.TotalOfferedPrice:N2} (เกินราคารับซื้อสูงสุด แนะนำให้ขออนุมัติจาก Owner/Admin)"
            : $"เสนอราคา ฿{po.TotalOfferedPrice:N2} สำเร็จ";
        return Success(po, msg);
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var po = await _purchaseService.ApprovePurchaseAsync(id);
        return Success(po, $"อนุมัติการรับซื้อใบรับซื้อ {po.PoNumber} เรียบร้อยแล้ว");
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var po = await _purchaseService.AcceptOfferAsync(id);
        return Success(po, $"ลูกค้าตกลงรับข้อเสนอราคา ฿{po.TotalAgreedPrice:N2}");
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var po = await _purchaseService.RejectOfferAsync(id);
        return Success(po, "ปฏิเสธ/ลูกค้ายกเลิกการขายเรียบร้อยแล้ว");
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordPurchasePaymentRequest request)
    {
        var po = await _purchaseService.RecordPaymentAsync(id, request);
        return Success(po, $"บันทึกการจ่ายเงิน ฿{request.Amount:N2} ให้ลูกค้าสำเร็จ");
    }

    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> ReceiveToStock(Guid id)
    {
        var po = await _purchaseService.ReceiveToStockAsync(id);
        return Success(po, $"นำสินค้าจากใบรับซื้อ {po.PoNumber} เข้าสู่คลังสินค้าเรียบร้อยแล้ว");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _purchaseService.DeletePurchaseOrderAsync(id);
        return Success("ลบใบรับซื้อสำเร็จ");
    }
}
