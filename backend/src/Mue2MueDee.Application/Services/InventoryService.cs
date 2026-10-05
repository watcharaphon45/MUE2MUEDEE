using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentStore _currentStore;
    private readonly ICurrentUser _currentUser;

    public InventoryService(
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
        {
            throw AppException.Forbidden("กรุณาเลือกร้านค้าก่อนทำรายการ");
        }
        return storeId.Value;
    }

    public async Task<PagedResult<InventoryItemDto>> GetInventoryPagedAsync(InventoryQueryFilter filter)
    {
        var storeId = RequireStoreId();
        var query = _context.InventoryItems
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .Include(i => i.Customer)
            .Where(i => i.StoreId == storeId);

        if (filter.BrandId.HasValue)
            query = query.Where(i => i.Product.BrandId == filter.BrandId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(i => i.Product.CategoryId == filter.CategoryId.Value);

        if (filter.ProductId.HasValue)
            query = query.Where(i => i.ProductId == filter.ProductId.Value);

        if (filter.Status.HasValue)
            query = query.Where(i => i.Status == filter.Status.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(i => i.SellingPrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(i => i.SellingPrice <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(i =>
                i.Sku.ToLower().Contains(s) ||
                i.SerialNumber.ToLower().Contains(s) ||
                i.Product.ModelName.ToLower().Contains(s) ||
                i.Product.ProductCode.ToLower().Contains(s) ||
                i.Product.Brand.Name.ToLower().Contains(s) ||
                (i.Accessories != null && i.Accessories.ToLower().Contains(s)) ||
                (i.Defects != null && i.Defects.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderByDescending(i => i.ReceivedAt)
            .ThenByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResult<InventoryItemDto>(dtos, totalCount, page, pageSize, totalPages);
    }

    public async Task<InventoryItemDto> GetInventoryItemByIdAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == id);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {id}");

        return MapToDto(item);
    }

    public async Task<InventoryItemDto> GetInventoryItemBySkuOrSerialAsync(string code)
    {
        var storeId = RequireStoreId();
        var normalized = code.Trim();

        var item = await _context.InventoryItems
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.StoreId == storeId &&
                (i.Sku.ToLower() == normalized.ToLower() || i.SerialNumber.ToLower() == normalized.ToLower()));

        if (item == null)
            throw AppException.NotFound($"ไม่พบสินค้าที่ตรงกับ SKU หรือ Serial Number '{normalized}'");

        return MapToDto(item);
    }

    public async Task<InventoryItemDto> CreateInventoryItemAsync(CreateInventoryItemRequest request)
    {
        var storeId = RequireStoreId();

        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.StoreId == storeId && p.Id == request.ProductId);

        if (product == null)
            throw AppException.NotFound($"ไม่พบสินค้ามาสเตอร์รหัส {request.ProductId}");

        var grade = await _context.ConditionGrades
            .FirstOrDefaultAsync(cg => cg.StoreId == storeId && cg.Id == request.ConditionGradeId);

        if (grade == null)
            throw AppException.NotFound($"ไม่พบเกรดสภาพรหัส {request.ConditionGradeId}");

        var serial = request.SerialNumber.Trim();
        var duplicateSerial = await _context.InventoryItems
            .AnyAsync(i => i.StoreId == storeId && i.ProductId == request.ProductId && i.SerialNumber.ToLower() == serial.ToLower());

        if (duplicateSerial)
            throw AppException.Conflict($"สินค้า {product.ModelName} หมายเลขซีเรียล '{serial}' มีอยู่ในระบบของร้านแล้ว");

        if (request.CustomerId.HasValue)
        {
            var customerExists = await _context.Customers.AnyAsync(c => c.StoreId == storeId && c.Id == request.CustomerId.Value);
            if (!customerExists)
                throw AppException.NotFound($"ไม่พบข้อมูลลูกค้ารหัส {request.CustomerId.Value}");
        }

        // Generate SKU
        var categoryPrefix = GetCategoryPrefix(product.Category.Type);
        var yearMonth = DateTime.UtcNow.ToString("yyMM");
        var latestCount = await _context.InventoryItems
            .CountAsync(i => i.StoreId == storeId && i.Sku.StartsWith($"{categoryPrefix}-{yearMonth}-"));
        var nextSequence = latestCount + 1;
        var sku = $"{categoryPrefix}-{yearMonth}-{nextSequence:D4}";

        var actualCost = request.ActualCost.HasValue && request.ActualCost.Value > 0
            ? request.ActualCost.Value
            : request.PurchaseCost;

        var receivedAt = request.ReceivedAt ?? DateTimeOffset.UtcNow;

        var item = new InventoryItem
        {
            StoreId = storeId,
            Sku = sku,
            ProductId = product.Id,
            SerialNumber = serial,
            ConditionGradeId = grade.Id,
            Status = InventoryStatus.Available,
            PurchaseCost = request.PurchaseCost,
            ActualCost = actualCost,
            SellingPrice = request.SellingPrice,
            ShutterCount = request.ShutterCount,
            Accessories = request.Accessories?.Trim(),
            Defects = request.Defects?.Trim(),
            Notes = request.Notes?.Trim(),
            ReceivedAt = receivedAt,
            CustomerId = request.CustomerId
        };

        _context.InventoryItems.Add(item);

        // Add Initial Stock Movement: Receive
        var movement = new StockMovement
        {
            StoreId = storeId,
            InventoryItemId = item.Id,
            MovementType = StockMovementType.Receive,
            Quantity = 1,
            ReferenceNumber = sku,
            Notes = $"รับสินค้าเข้าคลัง ต้นทุนรับซื้อ ฿{request.PurchaseCost:N2} (ราคาตั้งขาย ฿{request.SellingPrice:N2})",
            CreatedBy = _currentUser.Username ?? "system",
            CreatedAt = receivedAt
        };

        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();

        // Attach navigations for DTO mapping
        item.Product = product;
        item.ConditionGrade = grade;
        if (request.CustomerId.HasValue)
        {
            item.Customer = await _context.Customers.FindAsync(request.CustomerId.Value);
        }

        return MapToDto(item);
    }

    public async Task<InventoryItemDto> UpdateInventoryItemAsync(Guid id, UpdateInventoryItemRequest request)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == id);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {id}");

        var serial = request.SerialNumber.Trim();
        var duplicateSerial = await _context.InventoryItems
            .AnyAsync(i => i.StoreId == storeId && i.Id != id && i.ProductId == item.ProductId && i.SerialNumber.ToLower() == serial.ToLower());

        if (duplicateSerial)
            throw AppException.Conflict($"สินค้า {item.Product.ModelName} หมายเลขซีเรียล '{serial}' มีอยู่ในระบบแล้ว");

        if (item.ConditionGradeId != request.ConditionGradeId)
        {
            var grade = await _context.ConditionGrades
                .FirstOrDefaultAsync(cg => cg.StoreId == storeId && cg.Id == request.ConditionGradeId);
            if (grade == null)
                throw AppException.NotFound($"ไม่พบเกรดสภาพรหัส {request.ConditionGradeId}");
            item.ConditionGrade = grade;
            item.ConditionGradeId = grade.Id;
        }

        item.SerialNumber = serial;
        item.SellingPrice = request.SellingPrice;
        item.ShutterCount = request.ShutterCount;
        item.Accessories = request.Accessories?.Trim();
        item.Defects = request.Defects?.Trim();
        item.Notes = request.Notes?.Trim();

        await _context.SaveChangesAsync();

        return MapToDto(item);
    }

    public async Task<InventoryItemDto> AdjustCostAsync(Guid id, AdjustInventoryCostRequest request)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == id);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {id}");

        if (request.AdditionalCost <= 0)
            throw AppException.BadRequest("จำนวนเงินต้นทุนเพิ่มเติมต้องมากกว่า 0 บาท");

        var oldCost = item.ActualCost;
        item.ActualCost += request.AdditionalCost;

        // Record stock movement for cost adjustment
        var movement = new StockMovement
        {
            StoreId = storeId,
            InventoryItemId = item.Id,
            MovementType = StockMovementType.CostAdjustment,
            Quantity = 0,
            Notes = $"ปรับเพิ่มต้นทุน: +฿{request.AdditionalCost:N2} ({request.CostTypeReason}) [ต้นทุนเดิม: ฿{oldCost:N2} ➔ ใหม่: ฿{item.ActualCost:N2}] {request.Notes}".Trim(),
            CreatedBy = _currentUser.Username ?? "system",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        return MapToDto(item);
    }

    public async Task<InventoryItemDto> UpdateStatusAsync(Guid id, UpdateInventoryStatusRequest request)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == id);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {id}");

        if (item.Status == request.NewStatus)
            return MapToDto(item);

        var oldStatus = item.Status;
        item.Status = request.NewStatus;

        StockMovementType movementType;
        int qty = 0;

        switch (request.NewStatus)
        {
            case InventoryStatus.Reserved:
                movementType = StockMovementType.Reserve;
                break;
            case InventoryStatus.Available when oldStatus == InventoryStatus.Reserved:
                movementType = StockMovementType.CancelReservation;
                break;
            case InventoryStatus.Sold:
                movementType = StockMovementType.Sell;
                qty = -1;
                item.SoldAt = DateTimeOffset.UtcNow;
                break;
            case InventoryStatus.Returned:
                movementType = StockMovementType.Return;
                qty = 1;
                item.SoldAt = null;
                break;
            case InventoryStatus.WrittenOff:
            case InventoryStatus.Damaged:
                movementType = StockMovementType.WriteOff;
                qty = -1;
                break;
            default:
                movementType = StockMovementType.StatusChange;
                break;
        }

        var movement = new StockMovement
        {
            StoreId = storeId,
            InventoryItemId = item.Id,
            MovementType = movementType,
            Quantity = qty,
            ReferenceNumber = request.ReferenceNumber,
            Notes = $"เปลี่ยนสถานะจาก '{FormatStatus(oldStatus)}' ➔ '{FormatStatus(request.NewStatus)}'. {request.Reason}".Trim(),
            CreatedBy = _currentUser.Username ?? "system",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        return MapToDto(item);
    }

    public async Task<IReadOnlyList<StockMovementDto>> GetStockMovementsAsync(Guid inventoryItemId)
    {
        var storeId = RequireStoreId();
        var exists = await _context.InventoryItems.AnyAsync(i => i.StoreId == storeId && i.Id == inventoryItemId);
        if (!exists)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {inventoryItemId}");

        var movements = await _context.StockMovements
            .Where(sm => sm.StoreId == storeId && sm.InventoryItemId == inventoryItemId)
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

        return movements;
    }

    public async Task DeleteInventoryItemAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == id);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {id}");

        if (item.Status == InventoryStatus.Sold)
            throw AppException.BadRequest("ไม่สามารถลบสินค้าที่มีสถานะขายแล้วได้");

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // Mapping & Helpers
    // ==========================================

    private static InventoryItemDto MapToDto(InventoryItem i)
    {
        var profit = i.SellingPrice - i.ActualCost;
        var margin = i.ActualCost > 0
            ? Math.Round((profit / i.ActualCost) * 100, 2)
            : 0;

        var ageDays = (int)(DateTimeOffset.UtcNow - i.ReceivedAt).TotalDays;

        return new InventoryItemDto(
            i.Id,
            i.Sku,
            i.ProductId,
            i.Product?.ModelName ?? "",
            i.Product?.ProductCode ?? "",
            i.Product?.Brand?.Name ?? "",
            i.Product?.Category?.Name ?? "",
            i.SerialNumber,
            i.ConditionGradeId,
            i.ConditionGrade?.GradeCode ?? "",
            i.ConditionGrade?.GradeName ?? "",
            i.Status,
            FormatStatus(i.Status),
            i.PurchaseCost,
            i.ActualCost,
            i.SellingPrice,
            profit,
            margin,
            i.ShutterCount,
            i.Accessories,
            i.Defects,
            i.Notes,
            i.ReceivedAt,
            i.SoldAt,
            i.CustomerId,
            i.Customer?.Name,
            ageDays,
            i.CreatedAt
        );
    }

    private static string GetCategoryPrefix(CategoryType type) => type switch
    {
        CategoryType.Camera => "CAM",
        CategoryType.Lens => "LEN",
        CategoryType.Flash => "FLA",
        CategoryType.Accessory => "ACC",
        _ => "ITM"
    };

    private static string FormatStatus(InventoryStatus status) => status switch
    {
        InventoryStatus.Draft => "ร่างรายการ",
        InventoryStatus.PendingInspection => "รอตรวจเช็คสภาพ",
        InventoryStatus.Available => "พร้อมขาย (ในสต็อก)",
        InventoryStatus.Reserved => "จองแล้ว",
        InventoryStatus.Sold => "ขายแล้ว",
        InventoryStatus.Repair => "ส่งซ่อม/ทำความสะอาด",
        InventoryStatus.Returned => "ลูกค้านำมาคืน",
        InventoryStatus.Damaged => "ชำรุดเสียหาย",
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
        StockMovementType.CostAdjustment => "ปรับเพิ่มต้นทุน",
        StockMovementType.WriteOff => "ตัดสต็อกจำหน่าย",
        _ => type.ToString()
    };
}
