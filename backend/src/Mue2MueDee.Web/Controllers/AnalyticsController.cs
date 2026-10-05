using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[Authorize]
[Route("api/analytics")]
public class AnalyticsController : ApiControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        var result = await _analyticsService.GetDashboardMetricsAsync();
        return Success(result, "ดึงข้อมูลสถิติภาพรวมสำเร็จ");
    }

    [HttpGet("stock-aging")]
    public async Task<IActionResult> GetStockAgingReport()
    {
        var result = await _analyticsService.GetStockAgingReportAsync();
        return Success(result, "ดึงข้อมูลวิเคราะห์อายุสต็อกสำเร็จ");
    }

    [HttpGet("sales-report")]
    public async Task<IActionResult> GetSalesReport([FromQuery] SalesReportQuery query)
    {
        var result = await _analyticsService.GetSalesReportAsync(query);
        return Success(result, "ดึงรายงานยอดขายและผลกำไรสำเร็จ");
    }

    [HttpGet("sales-report/export-csv")]
    public async Task<IActionResult> ExportSalesReportCsv([FromQuery] SalesReportQuery query)
    {
        var bytes = await _analyticsService.ExportSalesReportCsvAsync(query);
        var filename = $"sales_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", filename);
    }

    [HttpGet("stock-aging/export-csv")]
    public async Task<IActionResult> ExportStockAgingCsv()
    {
        var bytes = await _analyticsService.ExportStockAgingCsvAsync();
        var filename = $"stock_aging_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", filename);
    }
}
