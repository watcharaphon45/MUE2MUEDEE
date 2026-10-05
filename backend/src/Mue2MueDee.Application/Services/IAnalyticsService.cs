using Mue2MueDee.Application.DTOs;

namespace Mue2MueDee.Application.Services;

public interface IAnalyticsService
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync();
    Task<StockAgingReportDto> GetStockAgingReportAsync();
    Task<SalesReportDto> GetSalesReportAsync(SalesReportQuery query);
    Task<byte[]> ExportSalesReportCsvAsync(SalesReportQuery query);
    Task<byte[]> ExportStockAgingCsvAsync();
}
