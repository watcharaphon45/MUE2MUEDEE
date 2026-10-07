using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentStore _currentStore;
    private readonly IPricingService _pricingService;

    public AnalyticsService(
        IApplicationDbContext context,
        ICurrentStore currentStore,
        IPricingService pricingService)
    {
        _context = context;
        _currentStore = currentStore;
        _pricingService = pricingService;
    }

    private Guid RequireStoreId()
    {
        if (!_currentStore.StoreId.HasValue)
        {
            throw AppException.BadRequest("กรุณาเลือกร้านค้าก่อนเข้าถึงข้อมูลสถิติและรายงาน");
        }
        return _currentStore.StoreId.Value;
    }

    private bool IsFinancialAuthorized()
    {
        return _currentStore.Role == StoreRole.Owner || _currentStore.Role == StoreRole.Admin;
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
    {
        var storeId = RequireStoreId();
        var isAuthorized = IsFinancialAuthorized();
        var now = DateTimeOffset.UtcNow;
        var startOfToday = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        // Inventory status counts
        var inventoryItems = await _context.InventoryItems
            .AsNoTracking()
            .Where(i => i.StoreId == storeId)
            .Select(i => new
            {
                i.Id,
                i.Status,
                i.ReceivedAt,
                i.ActualCost,
                i.SellingPrice
            })
            .ToListAsync();

        var availableItems = inventoryItems.Count(i => i.Status == InventoryStatus.Available);
        var pendingInspection = inventoryItems.Count(i => i.Status == InventoryStatus.PendingInspection);
        var repairCount = inventoryItems.Count(i => i.Status == InventoryStatus.Repair);
        var reservedCount = inventoryItems.Count(i => i.Status == InventoryStatus.Reserved);
        var totalActive = inventoryItems.Count(i => i.Status != InventoryStatus.Sold);

        // Aging counts (for unsold items)
        var unsoldItems = inventoryItems.Where(i => i.Status != InventoryStatus.Sold).ToList();
        var agingOver60 = unsoldItems.Count(i => (now - i.ReceivedAt).TotalDays >= 61);
        var agingOver90 = unsoldItems.Count(i => (now - i.ReceivedAt).TotalDays >= 91);
        var agingOver120 = unsoldItems.Count(i => (now - i.ReceivedAt).TotalDays >= 121);

        // Financial values
        decimal? inventoryCostValue = isAuthorized ? unsoldItems.Sum(i => i.ActualCost) : null;
        decimal? inventorySalesValue = isAuthorized ? unsoldItems.Sum(i => i.SellingPrice) : null;
        decimal? deadCapitalCost = isAuthorized
            ? unsoldItems.Where(i => (now - i.ReceivedAt).TotalDays >= 91).Sum(i => i.ActualCost)
            : null;

        // Sales figures
        var salesOrders = await _context.SaleOrders
            .AsNoTracking()
            .Where(s => s.StoreId == storeId && s.Status == SaleStatus.Completed)
            .Select(s => new
            {
                s.Id,
                s.SaleDate,
                s.NetAmount,
                s.TotalCost,
                s.TotalGrossProfit,
                s.TotalMarginPercentage
            })
            .ToListAsync();

        var salesTodayOrders = salesOrders.Where(s => s.SaleDate >= startOfToday).ToList();
        var salesTodayAmount = salesTodayOrders.Sum(s => s.NetAmount);
        var salesTodayCount = salesTodayOrders.Count;

        var salesThisMonthOrders = salesOrders.Where(s => s.SaleDate >= startOfMonth).ToList();
        var salesThisMonthAmount = salesThisMonthOrders.Sum(s => s.NetAmount);
        var salesThisMonthCount = salesThisMonthOrders.Count;
        var soldThisMonthCount = await _context.SaleItems
            .AsNoTracking()
            .Where(si => si.SaleOrder.StoreId == storeId &&
                         si.SaleOrder.Status == SaleStatus.Completed &&
                         si.SaleOrder.SaleDate >= startOfMonth)
            .CountAsync();

        var averageOrderValue = salesThisMonthCount > 0
            ? Math.Round(salesThisMonthAmount / salesThisMonthCount, 2)
            : 0m;

        decimal? grossProfitThisMonth = isAuthorized ? salesThisMonthOrders.Sum(s => s.TotalGrossProfit) : null;
        decimal? averageProfitMarginThisMonth = null;
        if (isAuthorized && salesThisMonthAmount > 0 && grossProfitThisMonth.HasValue)
        {
            averageProfitMarginThisMonth = Math.Round((grossProfitThisMonth.Value / salesThisMonthAmount) * 100.0m, 2);
        }

        // Recent Sales
        var recentSales = await _context.SaleOrders
            .AsNoTracking()
            .Where(s => s.StoreId == storeId)
            .OrderByDescending(s => s.SaleDate)
            .Take(5)
            .Select(s => new RecentSaleDto(
                s.Id,
                s.SaleNumber,
                s.Customer != null ? s.Customer.Name : (s.CustomerName ?? "ลูกค้าหน้าร้าน"),
                s.Items.Count,
                s.NetAmount,
                isAuthorized ? (decimal?)s.TotalGrossProfit : null,
                s.SaleDate,
                s.Status
            ))
            .ToListAsync();

        // Recent Purchases
        var recentPurchases = await _context.PurchaseOrders
            .AsNoTracking()
            .Where(p => p.StoreId == storeId)
            .OrderByDescending(p => p.PurchaseDate)
            .Take(5)
            .Select(p => new RecentPurchaseDto(
                p.Id,
                p.PoNumber,
                p.Customer != null ? p.Customer.Name : "ผู้ขายทั่วไป",
                p.Items.Count,
                p.TotalAgreedPrice,
                p.PurchaseDate,
                p.Status
            ))
            .ToListAsync();

        return new DashboardMetricsDto(
            AvailableItems: availableItems,
            PendingInspection: pendingInspection,
            RepairCount: repairCount,
            ReservedCount: reservedCount,
            TotalActiveStockCount: totalActive,
            SoldThisMonthCount: soldThisMonthCount,
            SalesTodayAmount: salesTodayAmount,
            SalesTodayCount: salesTodayCount,
            SalesThisMonthAmount: salesThisMonthAmount,
            SalesThisMonthCount: salesThisMonthCount,
            AverageOrderValue: averageOrderValue,
            StockAgingOver60DaysCount: agingOver60,
            StockAgingOver90DaysCount: agingOver90,
            StockAgingOver120DaysCount: agingOver120,
            IsFinancialVisible: isAuthorized,
            InventoryCostValue: inventoryCostValue,
            InventorySalesValue: inventorySalesValue,
            GrossProfitThisMonth: grossProfitThisMonth,
            AverageProfitMarginThisMonth: averageProfitMarginThisMonth,
            DeadCapitalCost: deadCapitalCost,
            RecentSales: recentSales,
            RecentPurchases: recentPurchases
        );
    }

    public async Task<StockAgingReportDto> GetStockAgingReportAsync()
    {
        var storeId = RequireStoreId();
        var isAuthorized = IsFinancialAuthorized();
        var now = DateTimeOffset.UtcNow;

        var items = await _context.InventoryItems
            .AsNoTracking()
            .Include(i => i.Product)
                .ThenInclude(p => p.Brand)
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .Where(i => i.StoreId == storeId && i.Status != InventoryStatus.Sold)
            .OrderByDescending(i => i.ReceivedAt)
            .ToListAsync();

        int totalCount = items.Count;
        decimal? totalCost = isAuthorized ? items.Sum(i => i.ActualCost) : null;
        decimal totalSales = items.Sum(i => i.SellingPrice);

        // Bucket definitions
        var bucket030 = new List<InventoryItem>();
        var bucket3160 = new List<InventoryItem>();
        var bucket6190 = new List<InventoryItem>();
        var bucket91120 = new List<InventoryItem>();
        var bucketOver120 = new List<InventoryItem>();

        var agingDetails = new List<AgingItemDetailDto>();

        foreach (var item in items)
        {
            var days = Math.Max(0, (int)(now - item.ReceivedAt).TotalDays);
            var calc = _pricingService.CalculateFormula(item.ActualCost, 15.0m, days);

            if (days <= 30)
                bucket030.Add(item);
            else if (days <= 60)
                bucket3160.Add(item);
            else if (days <= 90)
                bucket6190.Add(item);
            else if (days <= 120)
                bucket91120.Add(item);
            else
                bucketOver120.Add(item);

            agingDetails.Add(new AgingItemDetailDto(
                InventoryItemId: item.Id,
                Sku: item.Sku,
                SerialNumber: item.SerialNumber,
                BrandName: item.Product?.Brand?.Name ?? "-",
                CategoryName: item.Product?.Category?.Name ?? "-",
                ModelName: item.Product?.ModelName ?? "-",
                DaysInStock: days,
                ReceivedAt: item.ReceivedAt,
                ConditionGrade: item.ConditionGrade?.GradeCode ?? "-",
                ActualCost: isAuthorized ? item.ActualCost : 0m,
                CurrentSellingPrice: item.SellingPrice,
                RecommendedClearancePrice: calc.RecommendedPrice,
                AgingBucket: calc.AgingBucket,
                AgingDiscountPercentage: calc.AgingDiscountPercentage,
                Status: item.Status
            ));
        }

        var buckets = new List<AgingBucketSummaryDto>
        {
            new AgingBucketSummaryDto(
                "0_30",
                "0 - 30 วัน",
                "fresh",
                "emerald",
                bucket030.Count,
                isAuthorized ? bucket030.Sum(i => i.ActualCost) : 0m,
                bucket030.Sum(i => i.SellingPrice),
                0m
            ),
            new AgingBucketSummaryDto(
                "31_60",
                "31 - 60 วัน",
                "normal",
                "teal",
                bucket3160.Count,
                isAuthorized ? bucket3160.Sum(i => i.ActualCost) : 0m,
                bucket3160.Sum(i => i.SellingPrice),
                0m
            ),
            new AgingBucketSummaryDto(
                "61_90",
                "61 - 90 วัน",
                "warning",
                "amber",
                bucket6190.Count,
                isAuthorized ? bucket6190.Sum(i => i.ActualCost) : 0m,
                bucket6190.Sum(i => i.SellingPrice),
                5.0m
            ),
            new AgingBucketSummaryDto(
                "91_120",
                "91 - 120 วัน",
                "danger",
                "orange",
                bucket91120.Count,
                isAuthorized ? bucket91120.Sum(i => i.ActualCost) : 0m,
                bucket91120.Sum(i => i.SellingPrice),
                10.0m
            ),
            new AgingBucketSummaryDto(
                "over_120",
                "120+ วัน",
                "critical",
                "rose",
                bucketOver120.Count,
                isAuthorized ? bucketOver120.Sum(i => i.ActualCost) : 0m,
                bucketOver120.Sum(i => i.SellingPrice),
                15.0m
            )
        };

        decimal? deadCapitalCost = isAuthorized
            ? (bucket91120.Sum(i => i.ActualCost) + bucketOver120.Sum(i => i.ActualCost))
            : null;

        return new StockAgingReportDto(
            TotalItemsInStock: totalCount,
            IsFinancialVisible: isAuthorized,
            TotalCostValue: totalCost,
            TotalSalesValue: totalSales,
            DeadCapitalCostValue: deadCapitalCost,
            Buckets: buckets,
            AgingItems: agingDetails.OrderByDescending(i => i.DaysInStock).ToList()
        );
    }

    public async Task<SalesReportDto> GetSalesReportAsync(SalesReportQuery query)
    {
        var storeId = RequireStoreId();
        var isAuthorized = IsFinancialAuthorized();

        var ordersQuery = _context.SaleOrders
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Payments)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Brand)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .Where(s => s.StoreId == storeId && s.Status == SaleStatus.Completed);

        if (query.StartDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(s => s.SaleDate >= query.StartDate.Value);
        }
        if (query.EndDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(s => s.SaleDate <= query.EndDate.Value);
        }

        var orders = await ordersQuery
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        // Filter by Brand / Category / Payment if requested
        if (query.BrandId.HasValue)
        {
            orders = orders.Where(o => o.Items.Any(i => i.Product?.BrandId == query.BrandId.Value)).ToList();
        }
        if (query.CategoryId.HasValue)
        {
            orders = orders.Where(o => o.Items.Any(i => i.Product?.CategoryId == query.CategoryId.Value)).ToList();
        }
        if (query.PaymentMethod.HasValue)
        {
            orders = orders.Where(o => o.Payments.Any(p => p.PaymentMethod == query.PaymentMethod.Value)).ToList();
        }

        // Summary metrics
        var orderCount = orders.Count;
        var allItems = orders.SelectMany(o => o.Items).ToList();
        var itemsSoldCount = allItems.Count;
        var totalGrossSales = orders.Sum(o => o.TotalAmount);
        var totalDiscount = orders.Sum(o => o.DiscountAmount);
        var totalNetSales = orders.Sum(o => o.NetAmount);
        decimal? totalCost = isAuthorized ? orders.Sum(o => o.TotalCost) : null;
        decimal? totalGrossProfit = isAuthorized ? orders.Sum(o => o.TotalGrossProfit) : null;
        decimal? avgMargin = null;
        if (isAuthorized && totalNetSales > 0 && totalGrossProfit.HasValue)
        {
            avgMargin = Math.Round((totalGrossProfit.Value / totalNetSales) * 100.0m, 2);
        }

        var summary = new SalesReportSummaryDto(
            OrderCount: orderCount,
            ItemsSoldCount: itemsSoldCount,
            TotalGrossSales: totalGrossSales,
            TotalDiscount: totalDiscount,
            TotalNetSales: totalNetSales,
            IsFinancialVisible: isAuthorized,
            TotalCost: totalCost,
            TotalGrossProfit: totalGrossProfit,
            AverageMarginPercentage: avgMargin
        );

        // Group by Brand
        var brandGroups = allItems
            .Where(i => i.Product?.Brand != null)
            .GroupBy(i => new
            {
                BrandId = i.Product!.BrandId,
                BrandName = i.Product!.Brand!.Name
            })
            .Select(g =>
            {
                var sales = g.Sum(x => x.SubTotal);
                decimal? cost = isAuthorized ? g.Sum(x => x.ActualCost) : null;
                decimal? profit = isAuthorized ? g.Sum(x => x.GrossProfit) : null;
                decimal? margin = isAuthorized && sales > 0 && profit.HasValue
                    ? Math.Round((profit.Value / sales) * 100.0m, 2)
                    : null;

                return new ReportGroupSummaryDto(
                    GroupId: g.Key.BrandId,
                    GroupName: g.Key.BrandName,
                    QuantitySold: g.Count(),
                    TotalSales: sales,
                    TotalCost: cost,
                    GrossProfit: profit,
                    MarginPercentage: margin
                );
            })
            .OrderByDescending(g => g.TotalSales)
            .ToList();

        // Group by Category
        var categoryGroups = allItems
            .Where(i => i.Product?.Category != null)
            .GroupBy(i => new
            {
                CategoryId = i.Product!.CategoryId,
                CategoryName = i.Product!.Category!.Name
            })
            .Select(g =>
            {
                var sales = g.Sum(x => x.SubTotal);
                decimal? cost = isAuthorized ? g.Sum(x => x.ActualCost) : null;
                decimal? profit = isAuthorized ? g.Sum(x => x.GrossProfit) : null;
                decimal? margin = isAuthorized && sales > 0 && profit.HasValue
                    ? Math.Round((profit.Value / sales) * 100.0m, 2)
                    : null;

                return new ReportGroupSummaryDto(
                    GroupId: g.Key.CategoryId,
                    GroupName: g.Key.CategoryName,
                    QuantitySold: g.Count(),
                    TotalSales: sales,
                    TotalCost: cost,
                    GrossProfit: profit,
                    MarginPercentage: margin
                );
            })
            .OrderByDescending(g => g.TotalSales)
            .ToList();

        // Payment Method breakdown
        var allPayments = orders.SelectMany(o => o.Payments).ToList();
        var totalPaymentsAmount = allPayments.Sum(p => p.Amount);
        var paymentBreakdown = allPayments
            .GroupBy(p => p.PaymentMethod)
            .Select(g =>
            {
                var amount = g.Sum(x => x.Amount);
                var percent = totalPaymentsAmount > 0
                    ? Math.Round((amount / totalPaymentsAmount) * 100.0m, 1)
                    : 0m;
                var methodName = g.Key switch
                {
                    PaymentMethod.Cash => "เงินสด (Cash)",
                    PaymentMethod.BankTransfer => "โอนเงินผ่านธนาคาร",
                    PaymentMethod.PromptPay => "พร้อมเพย์ (PromptPay)",
                    PaymentMethod.CreditCard => "บัตรเครดิต (Credit Card)",
                    _ => g.Key.ToString()
                };

                return new PaymentMethodBreakdownDto(
                    Method: g.Key,
                    MethodName: methodName,
                    TransactionCount: g.Count(),
                    TotalAmount: amount,
                    Percentage: percent
                );
            })
            .OrderByDescending(p => p.TotalAmount)
            .ToList();

        // Daily trend
        var dailyTrend = orders
            .GroupBy(o => o.SaleDate.Date)
            .Select(g => new DailySalesTrendDto(
                DateString: g.Key.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                Date: g.Key,
                OrderCount: g.Count(),
                NetSales: g.Sum(x => x.NetAmount),
                GrossProfit: isAuthorized ? (decimal?)g.Sum(x => x.TotalGrossProfit) : null
            ))
            .OrderBy(d => d.Date)
            .ToList();

        // Order list
        var orderDtos = orders.Select(o =>
        {
            var pMethods = string.Join(", ", o.Payments.Select(p => p.PaymentMethod.ToString()).Distinct());
            return new SalesReportOrderDto(
                OrderId: o.Id,
                OrderNumber: o.SaleNumber,
                OrderDate: o.SaleDate,
                CustomerName: o.Customer?.Name ?? (o.CustomerName ?? "ลูกค้าหน้าร้าน"),
                ItemCount: o.Items.Count,
                PaymentMethods: string.IsNullOrEmpty(pMethods) ? "-" : pMethods,
                NetAmount: o.NetAmount,
                GrossProfit: isAuthorized ? (decimal?)o.TotalGrossProfit : null,
                MarginPercentage: isAuthorized ? (decimal?)o.TotalMarginPercentage : null,
                Status: o.Status
            );
        }).ToList();

        return new SalesReportDto(
            Summary: summary,
            BrandBreakdown: brandGroups,
            CategoryBreakdown: categoryGroups,
            PaymentBreakdown: paymentBreakdown,
            DailyTrend: dailyTrend,
            Orders: orderDtos
        );
    }

    public async Task<byte[]> ExportSalesReportCsvAsync(SalesReportQuery query)
    {
        var report = await GetSalesReportAsync(query);
        var sb = new StringBuilder();

        // Append UTF-8 BOM so Excel displays Thai characters correctly
        var preamble = Encoding.UTF8.GetPreamble();

        sb.AppendLine("รายงานยอดขายและผลกำไร — MUE2MUEDEE");
        sb.AppendLine($"วันที่ออกรายงาน: {DateTimeOffset.UtcNow:dd/MM/yyyy HH:mm:ss} UTC");
        sb.AppendLine($"จำนวนบิลขายทั้งหมด: {report.Summary.OrderCount}, จำนวนสินค้าที่ขาย: {report.Summary.ItemsSoldCount}, ยอดขายสุทธิรวม: ฿{report.Summary.TotalNetSales:N2}");
        if (report.Summary.IsFinancialVisible)
        {
            sb.AppendLine($"ต้นทุนรวม: ฿{report.Summary.TotalCost:N2}, กำไรขั้นต้นรวม: ฿{report.Summary.TotalGrossProfit:N2}, อัตรามาร์จิ้นเฉลี่ย: {report.Summary.AverageMarginPercentage}%");
        }
        sb.AppendLine();

        // Header for orders
        if (report.Summary.IsFinancialVisible)
        {
            sb.AppendLine("เลขที่บิล,วันที่ขาย,ลูกค้า,จำนวนชิ้น,ช่องทางชำระ,ยอดขายสุทธิ(฿),กำไรขั้นต้น(฿),มาร์จิ้น(%),สถานะ");
            foreach (var o in report.Orders)
            {
                var profitStr = o.GrossProfit.HasValue ? o.GrossProfit.Value.ToString("F2") : "0.00";
                var marginStr = o.MarginPercentage.HasValue ? o.MarginPercentage.Value.ToString("F2") : "0.00";
                sb.AppendLine($"\"{o.OrderNumber}\",\"{o.OrderDate:dd/MM/yyyy HH:mm}\",\"{o.CustomerName}\",{o.ItemCount},\"{o.PaymentMethods}\",{o.NetAmount:F2},{profitStr},{marginStr},\"{o.Status}\"");
            }
        }
        else
        {
            sb.AppendLine("เลขที่บิล,วันที่ขาย,ลูกค้า,จำนวนชิ้น,ช่องทางชำระ,ยอดขายสุทธิ(฿),สถานะ");
            foreach (var o in report.Orders)
            {
                sb.AppendLine($"\"{o.OrderNumber}\",\"{o.OrderDate:dd/MM/yyyy HH:mm}\",\"{o.CustomerName}\",{o.ItemCount},\"{o.PaymentMethods}\",{o.NetAmount:F2},\"{o.Status}\"");
            }
        }

        var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[preamble.Length + contentBytes.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(contentBytes, 0, result, preamble.Length, contentBytes.Length);
        return result;
    }

    public async Task<byte[]> ExportStockAgingCsvAsync()
    {
        var report = await GetStockAgingReportAsync();
        var sb = new StringBuilder();
        var preamble = Encoding.UTF8.GetPreamble();

        sb.AppendLine("รายงานวิเคราะห์อายุสต็อกสินค้าและเงินจม — MUE2MUEDEE");
        sb.AppendLine($"วันที่ออกรายงาน: {DateTimeOffset.UtcNow:dd/MM/yyyy HH:mm:ss} UTC");
        sb.AppendLine($"จำนวนสินค้าในสต็อกทั้งหมด: {report.TotalItemsInStock} ชิ้น, มูลค่าราคาขายรวม: ฿{report.TotalSalesValue:N2}");
        if (report.IsFinancialVisible)
        {
            sb.AppendLine($"มูลค่าต้นทุนรวม: ฿{report.TotalCostValue:N2}, เงินจมในสินค้า >90 วัน: ฿{report.DeadCapitalCostValue:N2}");
        }
        sb.AppendLine();

        // Bucket summary
        sb.AppendLine("ช่วงอายุสต็อก,จำนวน(ชิ้น),ส่วนลดอายุสต็อก(%),มูลค่าราคาขาย(฿)" + (report.IsFinancialVisible ? ",มูลค่าต้นทุน(฿)" : ""));
        foreach (var b in report.Buckets)
        {
            var line = $"\"{b.Label}\",{b.Count},{b.DiscountPercentage}%,{b.TotalSalesValue:F2}";
            if (report.IsFinancialVisible)
            {
                line += $",{b.TotalCost:F2}";
            }
            sb.AppendLine(line);
        }
        sb.AppendLine();

        // Detailed aging items
        if (report.IsFinancialVisible)
        {
            sb.AppendLine("SKU,Serial Number,แบรนด์,หมวดหมู่,รุ่นสินค้า,เกรดสภาพ,วันที่รับเข้า,อายุในคลัง(วัน),ช่วงอายุ,ต้นทุนจริง(฿),ราคาขายปัจจุบัน(฿),ราคาแนะนำระบายสต็อก(฿),สถานะ");
            foreach (var item in report.AgingItems)
            {
                sb.AppendLine($"\"{item.Sku}\",\"{item.SerialNumber ?? "-"}\",\"{item.BrandName}\",\"{item.CategoryName}\",\"{item.ModelName}\",\"{item.ConditionGrade}\",\"{item.ReceivedAt:dd/MM/yyyy}\",{item.DaysInStock},\"{item.AgingBucket}\",{item.ActualCost:F2},{item.CurrentSellingPrice:F2},{item.RecommendedClearancePrice:F2},\"{item.Status}\"");
            }
        }
        else
        {
            sb.AppendLine("SKU,Serial Number,แบรนด์,หมวดหมู่,รุ่นสินค้า,เกรดสภาพ,วันที่รับเข้า,อายุในคลัง(วัน),ช่วงอายุ,ราคาขายปัจจุบัน(฿),ราคาแนะนำระบายสต็อก(฿),สถานะ");
            foreach (var item in report.AgingItems)
            {
                sb.AppendLine($"\"{item.Sku}\",\"{item.SerialNumber ?? "-"}\",\"{item.BrandName}\",\"{item.CategoryName}\",\"{item.ModelName}\",\"{item.ConditionGrade}\",\"{item.ReceivedAt:dd/MM/yyyy}\",{item.DaysInStock},\"{item.AgingBucket}\",{item.CurrentSellingPrice:F2},{item.RecommendedClearancePrice:F2},\"{item.Status}\"");
            }
        }

        var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[preamble.Length + contentBytes.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(contentBytes, 0, result, preamble.Length, contentBytes.Length);
        return result;
    }
}
