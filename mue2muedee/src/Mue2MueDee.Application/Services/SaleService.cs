using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public class SaleService : ISaleService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentStore _currentStore;
    private readonly ICurrentUser _currentUser;

    public SaleService(
        IApplicationDbContext context,
        ICurrentStore currentStore,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentStore = currentStore;
        _currentUser = currentUser;
    }

    private Guid RequireStoreId()
    {
        var storeId = _currentStore.StoreId;
        if (!storeId.HasValue)
            throw AppException.Forbidden("กรุณาเลือกร้านค้าก่อนทำรายการ");
        return storeId.Value;
    }

    public async Task<PagedResult<SaleOrderDto>> GetSalesPagedAsync(SaleQueryFilter filter)
    {
        var storeId = RequireStoreId();
        var query = _context.SaleOrders
            .Include(so => so.Customer)
            .Include(so => so.Items)
            .Include(so => so.Payments)
            .Where(so => so.StoreId == storeId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(so =>
                so.SaleNumber.ToLower().Contains(s) ||
                (so.CustomerName != null && so.CustomerName.ToLower().Contains(s)) ||
                (so.CustomerPhone != null && so.CustomerPhone.Contains(s)) ||
                so.Items.Any(i => i.SerialNumber.ToLower().Contains(s) || i.Sku.ToLower().Contains(s))
            );
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(so => so.Status == filter.Status.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(so => so.SaleDate >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(so => so.SaleDate <= filter.EndDate.Value);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var items = await query
            .OrderByDescending(so => so.SaleDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResult<SaleOrderDto>(dtos, totalCount, filter.Page, filter.PageSize, totalPages);
    }

    public async Task<SaleOrderDto> GetSaleByIdAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var order = await _context.SaleOrders
            .Include(so => so.Customer)
            .Include(so => so.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Brand)
            .Include(so => so.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Category)
            .Include(so => so.Items).ThenInclude(i => i.InventoryItem).ThenInclude(inv => inv.ConditionGrade)
            .Include(so => so.Payments)
            .FirstOrDefaultAsync(so => so.StoreId == storeId && so.Id == id);

        if (order == null)
            throw AppException.NotFound($"ไม่พบรายการขายรหัส {id}");

        return MapToDto(order);
    }

    public async Task<SaleOrderDto> CreateSaleAsync(CreateSaleOrderRequest request)
    {
        var storeId = RequireStoreId();

        if (request.Items == null || request.Items.Count == 0)
            throw AppException.BadRequest("กรุณาเลือกสินค้าอย่างน้อย 1 รายการเพื่อทำรายการขาย");

        if (request.Payments == null || request.Payments.Count == 0)
            throw AppException.BadRequest("กรุณาระบุข้อมูลการชำระเงินของลูกค้า");

        var saleNumber = await GenerateSaleNumberAsync(storeId);

        var saleOrder = new SaleOrder
        {
            StoreId = storeId,
            SaleNumber = saleNumber,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName?.Trim(),
            CustomerPhone = request.CustomerPhone?.Trim(),
            SaleDate = DateTimeOffset.UtcNow,
            Status = SaleStatus.Completed,
            Notes = request.Notes?.Trim(),
            CreatedBy = _currentUser.Username ?? "system"
        };

        decimal totalAmount = 0;
        decimal totalDiscount = 0;
        decimal totalCost = 0;

        foreach (var reqItem in request.Items)
        {
            var invItem = await _context.InventoryItems
                .Include(i => i.Product)
                .Include(i => i.ConditionGrade)
                .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == reqItem.InventoryItemId);

            if (invItem == null)
                throw AppException.NotFound($"ไม่พบสินค้าในคลังรหัส {reqItem.InventoryItemId}");

            if (invItem.Status == InventoryStatus.Sold)
                throw AppException.Conflict($"สินค้า SKU {invItem.Sku} (Serial: {invItem.SerialNumber}) ถูกขายไปแล้ว");

            if (invItem.Status != InventoryStatus.Available && invItem.Status != InventoryStatus.Reserved)
                throw AppException.BadRequest($"สินค้า SKU {invItem.Sku} อยู่ในสถานะ '{invItem.Status}' ไม่พร้อมสำหรับการขาย");

            var unitPrice = reqItem.UnitPrice > 0 ? reqItem.UnitPrice : invItem.SellingPrice;
            var discount = Math.Max(0, reqItem.Discount ?? 0);
            var subTotal = Math.Max(0, unitPrice - discount);
            var actualCost = invItem.ActualCost;
            var grossProfit = subTotal - actualCost;
            var margin = subTotal > 0 ? Math.Round((grossProfit / subTotal) * 100, 2) : 0;

            var saleItem = new SaleItem
            {
                StoreId = storeId,
                SaleOrderId = saleOrder.Id,
                InventoryItemId = invItem.Id,
                ProductId = invItem.ProductId,
                ProductModelName = invItem.Product?.ModelName ?? "",
                Sku = invItem.Sku,
                SerialNumber = invItem.SerialNumber,
                ConditionGradeCode = invItem.ConditionGrade?.GradeCode ?? "A",
                ActualCost = actualCost,
                UnitPrice = unitPrice,
                Discount = discount,
                SubTotal = subTotal,
                GrossProfit = grossProfit,
                MarginPercentage = margin,
                WarrantyDays = reqItem.WarrantyDays ?? 30
            };

            saleOrder.Items.Add(saleItem);

            totalAmount += unitPrice;
            totalDiscount += discount;
            totalCost += actualCost;

            // Update InventoryItem status to Sold
            invItem.Status = InventoryStatus.Sold;
            invItem.SoldAt = DateTimeOffset.UtcNow;

            // Record StockMovement: Sell (-1)
            var movement = new StockMovement
            {
                StoreId = storeId,
                InventoryItemId = invItem.Id,
                MovementType = StockMovementType.Sell,
                Quantity = -1,
                ReferenceNumber = saleNumber,
                Notes = $"ขายสินค้าตามใบเสร็จ {saleNumber} (ราคาขายสุทธิ ฿{subTotal:N2}, ต้นทุนจริง ฿{actualCost:N2}, กำไร ฿{grossProfit:N2})",
                CreatedBy = _currentUser.Username ?? "system",
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.StockMovements.Add(movement);
        }

        var netAmount = totalAmount - totalDiscount;
        var totalGrossProfit = netAmount - totalCost;
        var totalMargin = netAmount > 0 ? Math.Round((totalGrossProfit / netAmount) * 100, 2) : 0;

        saleOrder.TotalAmount = totalAmount;
        saleOrder.DiscountAmount = totalDiscount;
        saleOrder.NetAmount = netAmount;
        saleOrder.TotalCost = totalCost;
        saleOrder.TotalGrossProfit = totalGrossProfit;
        saleOrder.TotalMarginPercentage = totalMargin;

        // Process Payments
        decimal totalPaid = 0;
        foreach (var p in request.Payments)
        {
            if (p.Amount <= 0) continue;

            var payment = new SalePayment
            {
                StoreId = storeId,
                SaleOrderId = saleOrder.Id,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                ReferenceNumber = p.ReferenceNumber?.Trim(),
                Notes = p.Notes?.Trim(),
                PaidAt = DateTimeOffset.UtcNow,
                CreatedBy = _currentUser.Username ?? "system"
            };

            saleOrder.Payments.Add(payment);
            totalPaid += p.Amount;
        }

        if (totalPaid < netAmount)
        {
            throw AppException.BadRequest($"ยอดเงินที่ชำระ (฿{totalPaid:N2}) น้อยกว่ายอดรวมสุทธิที่ต้องชำระ (฿{netAmount:N2})");
        }

        _context.SaleOrders.Add(saleOrder);
        await _context.SaveChangesAsync();

        return await GetSaleByIdAsync(saleOrder.Id);
    }

    public async Task<SaleOrderDto> CancelSaleAsync(Guid id, string reason)
    {
        var storeId = RequireStoreId();
        var order = await _context.SaleOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.StoreId == storeId && so.Id == id);

        if (order == null)
            throw AppException.NotFound($"ไม่พบรายการขายรหัส {id}");

        if (order.Status == SaleStatus.Cancelled)
            throw AppException.BadRequest("รายการขายนี้ถูกยกเลิกไปแล้ว");

        order.Status = SaleStatus.Cancelled;
        order.Notes = $"{order.Notes} [ยกเลิกบิล: {reason}]".Trim();

        // Revert inventory items to Available
        foreach (var saleItem in order.Items)
        {
            var invItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == saleItem.InventoryItemId);

            if (invItem != null)
            {
                invItem.Status = InventoryStatus.Available;
                invItem.SoldAt = null;

                var movement = new StockMovement
                {
                    StoreId = storeId,
                    InventoryItemId = invItem.Id,
                    MovementType = StockMovementType.Return,
                    Quantity = 1,
                    ReferenceNumber = order.SaleNumber,
                    Notes = $"รับสินค้าคืนสต็อกจากการยกเลิกบิลขาย {order.SaleNumber}: {reason}",
                    CreatedBy = _currentUser.Username ?? "system",
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _context.StockMovements.Add(movement);
            }
        }

        await _context.SaveChangesAsync();
        return await GetSaleByIdAsync(id);
    }

    public async Task ReserveItemAsync(Guid inventoryItemId, string? customerName, string? notes)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == inventoryItemId);

        if (item == null)
            throw AppException.NotFound($"ไม่พบสินค้าในคลังรหัส {inventoryItemId}");

        if (item.Status == InventoryStatus.Sold)
            throw AppException.BadRequest("สินค้านี้ถูกขายไปแล้ว ไม่สามารถจองได้");

        if (item.Status == InventoryStatus.Reserved)
            throw AppException.BadRequest("สินค้านี้อยู่ในสถานะจองอยู่แล้ว");

        item.Status = InventoryStatus.Reserved;

        var movement = new StockMovement
        {
            StoreId = storeId,
            InventoryItemId = item.Id,
            MovementType = StockMovementType.Reserve,
            Quantity = 0,
            Notes = $"จองสินค้าให้ลูกค้า: {customerName ?? "หน้าร้าน"} {notes}".Trim(),
            CreatedBy = _currentUser.Username ?? "system",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();
    }

    public async Task ReleaseReservationAsync(Guid inventoryItemId, string? reason)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == inventoryItemId);

        if (item == null)
            throw AppException.NotFound($"ไม่พบสินค้าในคลังรหัส {inventoryItemId}");

        if (item.Status != InventoryStatus.Reserved)
            throw AppException.BadRequest("สินค้านี้ไม่ได้อยู่ในสถานะจอง");

        item.Status = InventoryStatus.Available;

        var movement = new StockMovement
        {
            StoreId = storeId,
            InventoryItemId = item.Id,
            MovementType = StockMovementType.CancelReservation,
            Quantity = 0,
            Notes = $"ยกเลิกการจองสินค้า: {reason ?? "ลูกค้าสละสิทธิ์"}".Trim(),
            CreatedBy = _currentUser.Username ?? "system",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();
    }

    public async Task<SerialTraceDto> GetSerialTraceAsync(string serialNumber)
    {
        var storeId = RequireStoreId();
        var s = serialNumber.Trim();

        var item = await _context.InventoryItems
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.SerialNumber.ToLower() == s.ToLower());

        if (item == null)
            throw AppException.NotFound($"ไม่พบประวัติสินค้าสำหรับ Serial Number '{serialNumber}'");

        // Find source PO if any
        var purchaseItem = await _context.PurchaseItems
            .Include(pi => pi.PurchaseOrder).ThenInclude(po => po.Customer)
            .FirstOrDefaultAsync(pi => pi.StoreId == storeId && (pi.InventoryItemId == item.Id || pi.SerialNumber.ToLower() == s.ToLower()));

        // Find sale item if any
        var saleItem = await _context.SaleItems
            .Include(si => si.SaleOrder)
            .FirstOrDefaultAsync(si => si.StoreId == storeId && si.InventoryItemId == item.Id);

        // Get movements
        var movements = await _context.StockMovements
            .Where(sm => sm.StoreId == storeId && sm.InventoryItemId == item.Id)
            .OrderByDescending(sm => sm.CreatedAt)
            .Select(sm => new StockMovementDto(
                sm.Id,
                sm.InventoryItemId,
                sm.MovementType,
                FormatMovementType(sm.MovementType),
                sm.Quantity,
                sm.ReferenceNumber,
                sm.Notes,
                sm.CreatedAt,
                sm.CreatedBy
            ))
            .ToListAsync();

        return new SerialTraceDto(
            item.SerialNumber,
            item.Id,
            item.Sku,
            item.Product?.ModelName ?? "",
            item.Product?.Brand?.Name ?? "",
            item.Product?.Category?.Name ?? "",
            item.ConditionGrade?.GradeCode ?? "",
            item.ConditionGrade?.GradeName ?? "",
            FormatInventoryStatus(item.Status),
            item.Customer?.Name ?? purchaseItem?.PurchaseOrder?.Customer?.Name ?? "รับซื้อหน้าร้าน",
            purchaseItem?.PurchaseOrder?.PoNumber,
            item.ReceivedAt,
            item.PurchaseCost,
            item.ActualCost,
            item.SellingPrice,
            saleItem?.SaleOrder?.CustomerName,
            saleItem?.SaleOrder?.SaleNumber,
            item.SoldAt,
            saleItem?.SubTotal,
            saleItem?.GrossProfit,
            saleItem?.MarginPercentage,
            movements
        );
    }

    private async Task<string> GenerateSaleNumberAsync(Guid storeId)
    {
        var prefix = $"SO-{DateTime.UtcNow:yyMM}-";
        var lastOrder = await _context.SaleOrders
            .Where(so => so.StoreId == storeId && so.SaleNumber.StartsWith(prefix))
            .OrderByDescending(so => so.SaleNumber)
            .Select(so => so.SaleNumber)
            .FirstOrDefaultAsync();

        int nextSeq = 1;
        if (!string.IsNullOrEmpty(lastOrder) && lastOrder.Length >= prefix.Length + 4)
        {
            var seqStr = lastOrder.Substring(prefix.Length, 4);
            if (int.TryParse(seqStr, out int currentSeq))
            {
                nextSeq = currentSeq + 1;
            }
        }

        return $"{prefix}{nextSeq:D4}";
    }

    private static SaleOrderDto MapToDto(SaleOrder so)
    {
        var items = so.Items.Select(i => new SaleItemDto(
            i.Id,
            i.SaleOrderId,
            i.InventoryItemId,
            i.ProductId,
            i.ProductModelName,
            i.Sku,
            i.SerialNumber,
            i.ConditionGradeCode,
            i.ActualCost,
            i.UnitPrice,
            i.Discount,
            i.SubTotal,
            i.GrossProfit,
            i.MarginPercentage,
            i.WarrantyDays
        )).ToList();

        var payments = so.Payments.Select(p => new SalePaymentDto(
            p.Id,
            p.SaleOrderId,
            p.PaymentMethod,
            FormatPaymentMethod(p.PaymentMethod),
            p.Amount,
            p.ReferenceNumber,
            p.PaidAt,
            p.Notes,
            p.CreatedBy
        )).ToList();

        return new SaleOrderDto(
            so.Id,
            so.SaleNumber,
            so.CustomerId,
            so.CustomerName,
            so.CustomerPhone,
            so.SaleDate,
            so.Status,
            FormatSaleStatus(so.Status),
            so.TotalAmount,
            so.DiscountAmount,
            so.NetAmount,
            so.TotalCost,
            so.TotalGrossProfit,
            so.TotalMarginPercentage,
            so.Notes,
            so.CreatedBy,
            so.CreatedAt,
            items,
            payments
        );
    }

    private static string FormatSaleStatus(SaleStatus status) => status switch
    {
        SaleStatus.Draft => "ร่างรายการ",
        SaleStatus.Completed => "เสร็จสมบูรณ์",
        SaleStatus.Cancelled => "ยกเลิกการขาย",
        SaleStatus.Refunded => "คืนเงินแล้ว",
        _ => status.ToString()
    };

    private static string FormatPaymentMethod(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "เงินสด (Cash)",
        PaymentMethod.BankTransfer => "โอนผ่านธนาคาร (Bank Transfer)",
        PaymentMethod.PromptPay => "พร้อมเพย์ (PromptPay)",
        PaymentMethod.CreditCard => "บัตรเครดิต (Credit Card)",
        _ => "อื่นๆ"
    };

    private static string FormatInventoryStatus(InventoryStatus status) => status switch
    {
        InventoryStatus.Draft => "ร่างรายการ",
        InventoryStatus.PendingInspection => "รอตรวจเช็คสภาพ",
        InventoryStatus.Available => "พร้อมขาย (ในสต็อก)",
        InventoryStatus.Reserved => "จองแล้ว",
        InventoryStatus.Sold => "ขายแล้ว",
        InventoryStatus.Repair => "ส่งซ่อม",
        InventoryStatus.Returned => "รับคืน",
        InventoryStatus.Damaged => "ชำรุด",
        InventoryStatus.WrittenOff => "ตัดจำหน่าย",
        _ => status.ToString()
    };

    private static string FormatMovementType(StockMovementType type) => type switch
    {
        StockMovementType.Receive => "รับสินค้าเข้าคลัง",
        StockMovementType.Reserve => "จองสินค้า",
        StockMovementType.CancelReservation => "ยกเลิกการจอง",
        StockMovementType.Sell => "ขายสินค้าออก",
        StockMovementType.Return => "รับคืนสินค้า",
        StockMovementType.StatusChange => "เปลี่ยนสถานะ",
        StockMovementType.CostAdjustment => "ปรับต้นทุน",
        StockMovementType.WriteOff => "ตัดสต็อกจำหน่าย",
        _ => type.ToString()
    };
}
