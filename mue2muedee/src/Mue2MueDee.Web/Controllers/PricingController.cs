using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class PricingController : ApiControllerBase
{
    private readonly IPricingService _pricingService;

    public PricingController(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    [HttpGet("inventory/{inventoryItemId:guid}/costs")]
    public async Task<IActionResult> GetCostBreakdown(Guid inventoryItemId)
    {
        var result = await _pricingService.GetCostBreakdownAsync(inventoryItemId);
        return Success(result, "ดึงข้อมูลการแจกแจงต้นทุนสินค้าสำเร็จ");
    }

    [HttpPost("inventory/{inventoryItemId:guid}/costs")]
    public async Task<IActionResult> AddCostItem(Guid inventoryItemId, [FromBody] AddCostItemRequest request)
    {
        var result = await _pricingService.AddCostItemAsync(inventoryItemId, request);
        return CreatedSuccess(result, "บันทึกรายการต้นทุนเพิ่มเติมสำเร็จ");
    }

    [HttpDelete("inventory/{inventoryItemId:guid}/costs/{costId:guid}")]
    public async Task<IActionResult> DeleteCostItem(Guid inventoryItemId, Guid costId)
    {
        await _pricingService.DeleteCostItemAsync(inventoryItemId, costId);
        return Success<object?>(null, "ลบรายการต้นทุนสำเร็จ");
    }

    [HttpGet("inventory/{inventoryItemId:guid}/pricing-recommendation")]
    public async Task<IActionResult> GetPricingRecommendation(Guid inventoryItemId, [FromQuery] decimal? customTargetMargin)
    {
        var result = await _pricingService.GetPricingRecommendationAsync(inventoryItemId, customTargetMargin);
        return Success(result, "คำนวณราคาขายแนะนำสำเร็จ");
    }

    [HttpPut("inventory/{inventoryItemId:guid}/price")]
    public async Task<IActionResult> UpdatePrice(Guid inventoryItemId, [FromBody] UpdatePriceRequest request)
    {
        var result = await _pricingService.UpdatePriceAsync(inventoryItemId, request);
        return Success(result, "ปรับราคาขายสินค้าเรียบร้อยแล้ว");
    }

    [HttpGet("inventory/{inventoryItemId:guid}/price-history")]
    public async Task<IActionResult> GetPriceHistories(Guid inventoryItemId)
    {
        var result = await _pricingService.GetPriceHistoriesAsync(inventoryItemId);
        return Success(result, "ดึงประวัติการปรับราคาสำเร็จ");
    }

    [HttpGet("pricing/formula-preview")]
    public IActionResult PreviewFormula([FromQuery] decimal actualCost, [FromQuery] decimal targetMarginPercent, [FromQuery] int agingDays)
    {
        var result = _pricingService.CalculateFormula(actualCost, targetMarginPercent, agingDays);
        return Success(result, "คำนวณราคาจำลองสำเร็จ");
    }
}
