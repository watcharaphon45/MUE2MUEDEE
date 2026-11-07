using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentStore _currentStore;
    private readonly ICurrentUser _currentUser;

    public PurchaseService(
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

    public async Task<PagedResult<PurchaseOrderDto>> GetPurchaseOrdersPagedAsync(PurchaseQueryFilter filter)
    {
        var storeId = RequireStoreId();
        var query = _context.PurchaseOrders
            .Include(po => po.Customer)
            .Include(po => po.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Brand)
            .Include(po => po.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Category)
            .Include(po => po.Items).ThenInclude(i => i.ConditionGrade)
            .Include(po => po.Items).ThenInclude(i => i.InventoryItem)
            .Include(po => po.Payments)
            .Where(po => po.StoreId == storeId);

        if (filter.Status.HasValue)
            query = query.Where(po => po.Status == filter.Status.Value);

        if (filter.CustomerId.HasValue)
            query = query.Where(po => po.CustomerId == filter.CustomerId.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(po => po.PurchaseDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(po => po.PurchaseDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(po =>
                po.PoNumber.ToLower().Contains(s) ||
                po.Customer.Name.ToLower().Contains(s) ||
                po.Customer.Phone.Contains(s) ||
                po.Items.Any(i => i.SerialNumber.ToLower().Contains(s) || i.Product.ModelName.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var orders = await query
            .OrderByDescending(po => po.PurchaseDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = orders.Select(MapToDto).ToList();

        return new PagedResult<PurchaseOrderDto>(dtos, totalCount, page, pageSize, totalPages);
    }

    public async Task<PurchaseOrderDto> GetPurchaseOrderByIdAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, id);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อสินค้ารหัส {id}");

        return MapToDto(po);
    }

    public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request)
    {
        var storeId = RequireStoreId();

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.StoreId == storeId && c.Id == request.CustomerId);

        if (customer == null)
            throw AppException.NotFound($"ไม่พบข้อมูลลูกค้ารหัส {request.CustomerId}");

        // Generate PO Number: PO-yyMM-xxxx
        var yearMonth = DateTime.UtcNow.ToString("yyMM");
        var prefix = $"PO-{yearMonth}-";
        var sequence = await SequentialCode.NextAsync(_context.PurchaseOrders.IgnoreQueryFilters()
            .Where(p => p.StoreId == storeId).Select(p => p.PoNumber), prefix);
        var poNumber = $"{prefix}{sequence:D4}";

        var po = new PurchaseOrder
        {
            StoreId = storeId,
            PoNumber = poNumber,
            CustomerId = customer.Id,
            Status = PurchaseStatus.Appraising,
            Notes = request.Notes?.Trim(),
            PurchaseDate = DateTimeOffset.UtcNow
        };

        if (request.InitialItems != null && request.InitialItems.Count > 0)
        {
            foreach (var itemReq in request.InitialItems)
            {
                var item = await BuildPurchaseItemAsync(storeId, itemReq);
                EnsureUniqueItem(po, item);
                po.Items.Add(item);
            }
            RecalculateTotals(po);
        }

        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(po.Id);
    }

    public async Task<PurchaseOrderDto> AddItemAsync(Guid poId, CreatePurchaseItemRequest request)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, poId);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {poId}");

        EnsureEditable(po);

        var item = await BuildPurchaseItemAsync(storeId, request);
        EnsureUniqueItem(po, item);
        item.PurchaseOrderId = po.Id;

        po.Items.Add(item);
        RecalculateTotals(po);

        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(poId);
    }

    public async Task<PurchaseOrderDto> RemoveItemAsync(Guid poId, Guid itemId)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, poId);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {poId}");

        EnsureEditable(po);

        var item = po.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าที่ต้องการลบ (รหัส {itemId})");

        po.Items.Remove(item);
        _context.PurchaseItems.Remove(item);
        RecalculateTotals(po);

        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(poId);
    }

    // ==========================================
    // INSPECTION
    // ==========================================

    public async Task<InspectionTemplateDto> GetInspectionTemplateAsync(CategoryType categoryType)
    {
        var storeId = RequireStoreId();
        var template = await _context.InspectionTemplates
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.StoreId == storeId && t.CategoryType == categoryType && t.IsActive);

        // If store doesn't have custom template, fall back to default template
        if (template == null)
        {
            template = await SeedDefaultTemplateIfMissingAsync(storeId, categoryType);
        }

        return new InspectionTemplateDto(
            template.Id,
            template.CategoryType,
            FormatCategoryType(template.CategoryType),
            template.Name,
            template.Description,
            template.IsActive,
            template.Items.OrderBy(i => i.SortOrder).Select(i => new InspectionTemplateItemDto(
                i.Id,
                i.CategoryGroup,
                i.Title,
                i.Description,
                i.SortOrder,
                i.IsCritical
            )).ToList()
        );
    }

    public async Task<PurchaseOrderDto> SaveInspectionResultsAsync(Guid poId, Guid itemId, SaveInspectionResultsRequest request)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, poId);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {poId}");

        EnsureEditable(po);

        var item = po.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้ารหัส {itemId} ในใบรับซื้อนี้");

        var templateIds = request.Results.Select(r => r.TemplateItemId).ToList();
        if (templateIds.Distinct().Count() != templateIds.Count || request.Results.Any(r => !Enum.IsDefined(r.Result)))
            throw AppException.BadRequest("รายการตรวจสภาพซ้ำหรือผลการตรวจไม่ถูกต้อง");
        var validCount = await _context.InspectionTemplateItems.CountAsync(t => templateIds.Contains(t.Id) &&
            t.Template.StoreId == storeId && t.Template.CategoryType == item.Product.Category.Type);
        if (validCount != templateIds.Count)
            throw AppException.BadRequest("รายการตรวจสภาพต้องเป็นแบบตรวจของร้านและประเภทสินค้านี้");

        // Clear existing results and replace with new
        var existingResults = await _context.InspectionResults
            .Where(r => r.PurchaseItemId == itemId)
            .ToListAsync();
        _context.InspectionResults.RemoveRange(existingResults);

        foreach (var res in request.Results)
        {
            var inspection = new InspectionResult
            {
                PurchaseItemId = itemId,
                InspectionTemplateItemId = res.TemplateItemId,
                Result = res.Result,
                Notes = res.Notes?.Trim(),
                PhotoUrl = res.PhotoUrl?.Trim()
            };
            _context.InspectionResults.Add(inspection);
        }

        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(poId);
    }

    // ==========================================
    // APPRAISAL & WORKFLOW
    // ==========================================

    public async Task<PurchaseOrderDto> MakeOfferAsync(Guid poId, MakeOfferRequest request)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, poId);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {poId}");

        EnsureEditable(po);
        if (request.TotalOfferedPrice <= 0 || decimal.Round(request.TotalOfferedPrice, 2) != request.TotalOfferedPrice)
            throw AppException.BadRequest("ราคาเสนอซื้อต้องมากกว่า 0 บาท และมีทศนิยมไม่เกิน 2 ตำแหน่ง");

        po.ApprovedBy = null;
        po.ApprovedAt = null;

        if (po.Items.Count == 0)
            throw AppException.BadRequest("ต้องมีรายการสินค้าอย่างน้อย 1 รายการก่อนทำการเสนอราคา");

        po.TotalOfferedPrice = request.TotalOfferedPrice;
        po.Notes = string.IsNullOrWhiteSpace(request.Notes) ? po.Notes : $"{po.Notes}\n[ข้อเสนอราคา]: {request.Notes}".Trim();

        // Check if offered price exceeds MaxBuyPrice
        if (po.TotalOfferedPrice > po.TotalMaxBuyPrice)
        {
            po.NeedsApproval = true;
            po.Status = PurchaseStatus.Offered;
        }
        else
        {
            po.NeedsApproval = false;
            po.Status = PurchaseStatus.Offered;
        }

        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(poId);
    }

    public async Task<PurchaseOrderDto> ApprovePurchaseAsync(Guid poId)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, poId);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {poId}");

        if (po.Status != PurchaseStatus.Offered || !po.NeedsApproval)
            throw AppException.BadRequest("อนุมัติได้เฉพาะใบรับซื้อที่เสนอราคาและรออนุมัติเท่านั้น");

        if (_currentStore.Role != StoreRole.Owner && _currentStore.Role != StoreRole.Admin)
            throw AppException.Forbidden("เฉพาะเจ้าของร้าน (Owner) หรือผู้ดูแล (Admin) เท่านั้นที่สามารถอนุมัติการรับซื้อเกินราคาสูงสุดได้");

        po.NeedsApproval = false;
        po.ApprovedBy = _currentUser.Username ?? "owner";
        po.ApprovedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(poId);
    }

    public async Task<PurchaseOrderDto> AcceptOfferAsync(Guid poId)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, poId);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {poId}");

        if (po.Status != PurchaseStatus.Offered || po.Items.Count == 0)
            throw AppException.BadRequest("ต้องเสนอราคาก่อนยืนยันการตกลงรับซื้อ");

        if (po.NeedsApproval)
            throw AppException.BadRequest("รายการนี้เสนอราคาสูงกว่าเกณฑ์ ต้องได้รับการอนุมัติจาก Owner/Admin ก่อนลูกค้าตกลง");

        po.Status = PurchaseStatus.Accepted;
        po.TotalAgreedPrice = po.TotalOfferedPrice;

        // Allocate the negotiated total to item costs, retaining the final rounding remainder.
        var items = po.Items.OrderBy(i => i.Id).ToList();
        var weights = items.Select(i => Math.Max(0, i.OfferedPrice > 0 ? i.OfferedPrice : i.MaxBuyPrice)).ToList();
        var totalWeight = weights.Sum();
        var remaining = po.TotalAgreedPrice;
        for (var index = 0; index < items.Count; index++)
        {
            var share = totalWeight > 0 ? weights[index] / totalWeight : 1m / items.Count;
            var amount = index == items.Count - 1 ? remaining :
                Math.Min(remaining, decimal.Round(po.TotalAgreedPrice * share, 2));
            items[index].AgreedPrice = amount;
            remaining -= amount;
        }

        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(poId);
    }

    public async Task<PurchaseOrderDto> RejectOfferAsync(Guid poId)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, poId);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {poId}");

        if (po.Status != PurchaseStatus.Offered)
            throw AppException.BadRequest("ยกเลิกข้อเสนอได้เฉพาะใบรับซื้อที่อยู่ระหว่างเสนอราคาเท่านั้น");

        po.Status = PurchaseStatus.Rejected;
        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(poId);
    }

    public async Task<PurchaseOrderDto> RecordPaymentAsync(Guid poId, RecordPurchasePaymentRequest request)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, poId);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {poId}");

        if (po.Status != PurchaseStatus.Accepted && po.Status != PurchaseStatus.Paid && po.Status != PurchaseStatus.Received)
            throw AppException.BadRequest("ต้องเป็นใบรับซื้อที่ลูกค้าตกลงรับข้อเสนอแล้วเท่านั้น จึงจะบันทึกการชำระเงินได้");

        if (request.Amount <= 0 || decimal.Round(request.Amount, 2) != request.Amount)
            throw AppException.BadRequest("ยอดจ่ายเงินต้องมากกว่า 0 บาท และมีทศนิยมไม่เกิน 2 ตำแหน่ง");
        if (!Enum.IsDefined(request.PaymentMethod))
            throw AppException.BadRequest("ช่องทางการจ่ายเงินไม่ถูกต้อง");
        var totalPaid = po.Payments.Sum(p => p.Amount) + request.Amount;
        if (totalPaid > po.TotalAgreedPrice)
            throw AppException.BadRequest("ยอดจ่ายเงินรวมต้องไม่เกินราคาตกลงรับซื้อ");

        var payment = new PurchasePayment
        {
            StoreId = storeId,
            PurchaseOrderId = po.Id,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            ReferenceNumber = request.ReferenceNumber?.Trim(),
            ProofUrl = request.ProofUrl?.Trim(),
            Notes = request.Notes?.Trim(),
            PaidAt = DateTimeOffset.UtcNow
        };

        _context.PurchasePayments.Add(payment);
        if (po.Status != PurchaseStatus.Received)
            po.Status = totalPaid == po.TotalAgreedPrice ? PurchaseStatus.Paid : PurchaseStatus.Accepted;

        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(poId);
    }

    public async Task<PurchaseOrderDto> ReceiveToStockAsync(Guid poId)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, poId);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {poId}");

        if (po.Status != PurchaseStatus.Paid && po.Status != PurchaseStatus.Accepted)
            throw AppException.BadRequest("สามารถรับสินค้าเข้าคลังได้เฉพาะรายการที่ตกลงหรือชำระเงินแล้วเท่านั้น");

        var pendingItems = po.Items.Where(i => !i.InventoryItemId.HasValue).ToList();
        var serials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in pendingItems)
        {
            if (!serials.Add($"{item.ProductId}:{item.SerialNumber.Trim()}"))
                throw AppException.Conflict($"มี Serial Number '{item.SerialNumber}' ซ้ำในใบรับซื้อ");
            if (await _context.InventoryItems.IgnoreQueryFilters().AnyAsync(i => i.StoreId == storeId &&
                i.ProductId == item.ProductId && i.SerialNumber.ToLower() == item.SerialNumber.ToLower()))
                throw AppException.Conflict($"Serial Number '{item.SerialNumber}' มีอยู่ในคลังของร้านแล้ว");
        }

        var nextSequences = new Dictionary<string, int>();
        foreach (var item in pendingItems)
        {
            // Auto generate SKU
            var categoryPrefix = GetCategoryPrefix(item.Product.Category.Type);
            var yearMonth = DateTime.UtcNow.ToString("yyMM");
            var prefix = $"{categoryPrefix}-{yearMonth}-";
            if (!nextSequences.TryGetValue(prefix, out var sequence))
                sequence = await SequentialCode.NextAsync(_context.InventoryItems.IgnoreQueryFilters()
                    .Where(i => i.StoreId == storeId).Select(i => i.Sku), prefix);
            var sku = $"{prefix}{sequence:D4}";
            nextSequences[prefix] = sequence + 1;

            var actualCost = item.AgreedPrice + item.EstimatedRepairCost + item.EstimatedCleaningCost;

            var inventoryItem = new InventoryItem
            {
                StoreId = storeId,
                Sku = sku,
                ProductId = item.ProductId,
                SerialNumber = item.SerialNumber,
                ConditionGradeId = item.ConditionGradeId,
                Status = InventoryStatus.Available,
                PurchaseCost = item.AgreedPrice,
                ActualCost = actualCost,
                SellingPrice = item.ExpectedSalePrice,
                ShutterCount = item.ShutterCount,
                Accessories = item.Accessories,
                Defects = item.Defects,
                Notes = $"รับเข้าสตอกจากใบรับซื้อ {po.PoNumber}",
                CustomerId = po.CustomerId,
                ReceivedAt = DateTimeOffset.UtcNow
            };

            _context.InventoryItems.Add(inventoryItem);

            if (item.EstimatedRepairCost > 0)
                _context.InventoryCosts.Add(new InventoryCost
                {
                    StoreId = storeId, InventoryItemId = inventoryItem.Id,
                    CostType = CostType.Repair, Amount = item.EstimatedRepairCost,
                    Description = $"ค่าซ่อมประมาณการจากใบรับซื้อ {po.PoNumber}",
                    IncurredAt = DateTimeOffset.UtcNow
                });
            if (item.EstimatedCleaningCost > 0)
                _context.InventoryCosts.Add(new InventoryCost
                {
                    StoreId = storeId, InventoryItemId = inventoryItem.Id,
                    CostType = CostType.Cleaning, Amount = item.EstimatedCleaningCost,
                    Description = $"ค่าทำความสะอาดประมาณการจากใบรับซื้อ {po.PoNumber}",
                    IncurredAt = DateTimeOffset.UtcNow
                });

            var movement = new StockMovement
            {
                StoreId = storeId,
                InventoryItemId = inventoryItem.Id,
                MovementType = StockMovementType.Receive,
                Quantity = 1,
                ReferenceNumber = po.PoNumber,
                Notes = $"รับสินค้าเข้าสตอกจากใบรับซื้อ {po.PoNumber} (ราคาซื้อตกลง ฿{item.AgreedPrice:N2}, ต้นทุนจริง ฿{actualCost:N2})",
                CreatedBy = _currentUser.Username ?? "system",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.StockMovements.Add(movement);

            item.InventoryItemId = inventoryItem.Id;
        }

        po.Status = PurchaseStatus.Received;
        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(poId);
    }

    public async Task DeletePurchaseOrderAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var po = await LoadPurchaseOrderEntityAsync(storeId, id);

        if (po == null)
            throw AppException.NotFound($"ไม่พบใบรับซื้อรหัส {id}");

        if (po.Status == PurchaseStatus.Received || po.Payments.Count > 0)
            throw AppException.BadRequest("ไม่สามารถลบใบรับซื้อที่นำสินค้าเข้าคลังหรือบันทึกจ่ายเงินแล้วได้");

        _context.PurchaseOrders.Remove(po);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // Helper Methods
    // ==========================================

    private async Task<PurchaseOrder?> LoadPurchaseOrderEntityAsync(Guid storeId, Guid poId)
    {
        return await _context.PurchaseOrders
            .Include(po => po.Customer)
            .Include(po => po.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Brand)
            .Include(po => po.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Category)
            .Include(po => po.Items).ThenInclude(i => i.ConditionGrade)
            .Include(po => po.Items).ThenInclude(i => i.InventoryItem)
            .Include(po => po.Items).ThenInclude(i => i.InspectionResults).ThenInclude(ir => ir.InspectionTemplateItem)
            .Include(po => po.Payments)
            .FirstOrDefaultAsync(po => po.StoreId == storeId && po.Id == poId);
    }

    private async Task<PurchaseItem> BuildPurchaseItemAsync(Guid storeId, CreatePurchaseItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SerialNumber) || request.ExpectedSalePrice < 0 ||
            request.EstimatedRepairCost < 0 || request.EstimatedCleaningCost < 0 ||
            request.MissingAccessoriesDeduction < 0 || request.OfferedPrice < 0 || request.ShutterCount < 0)
            throw AppException.BadRequest("กรุณาระบุ Serial Number และราคาหรือจำนวนที่ไม่ติดลบ");

        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.StoreId == storeId && p.Id == request.ProductId);

        if (product == null)
            throw AppException.NotFound($"ไม่พบรุ่นสินค้ารหัส {request.ProductId}");

        var grade = await _context.ConditionGrades
            .FirstOrDefaultAsync(g => g.StoreId == storeId && g.Id == request.ConditionGradeId);

        if (grade == null)
            throw AppException.NotFound($"ไม่พบเกรดสภาพรหัส {request.ConditionGradeId}");

        var serial = request.SerialNumber.Trim();
        if (await _context.InventoryItems.IgnoreQueryFilters().AnyAsync(i => i.StoreId == storeId &&
            i.ProductId == product.Id && i.SerialNumber.ToLower() == serial.ToLower()))
            throw AppException.Conflict($"Serial Number '{serial}' มีอยู่ในคลังของร้านแล้ว");

        // Formula for MaxBuyPrice:
        // MaxBuy = ExpectedSale * (1 - 0.15) - Repair - Cleaning - MissingAccessories
        var maxBuyPrice = CalculateMaxBuyPrice(
            request.ExpectedSalePrice,
            request.EstimatedRepairCost,
            request.EstimatedCleaningCost,
            request.MissingAccessoriesDeduction,
            0.15m // Target Margin 15%
        );

        var offeredPrice = request.OfferedPrice.HasValue && request.OfferedPrice.Value > 0
            ? request.OfferedPrice.Value
            : maxBuyPrice;

        return new PurchaseItem
        {
            StoreId = storeId,
            ProductId = product.Id,
            SerialNumber = serial,
            ConditionGradeId = grade.Id,
            ShutterCount = request.ShutterCount,
            Accessories = request.Accessories?.Trim(),
            Defects = request.Defects?.Trim(),
            ExpectedSalePrice = request.ExpectedSalePrice,
            EstimatedRepairCost = request.EstimatedRepairCost,
            EstimatedCleaningCost = request.EstimatedCleaningCost,
            MissingAccessoriesDeduction = request.MissingAccessoriesDeduction,
            MaxBuyPrice = maxBuyPrice,
            OfferedPrice = offeredPrice,
            AgreedPrice = 0
        };
    }

    public static decimal CalculateMaxBuyPrice(
        decimal expectedSale,
        decimal repair,
        decimal cleaning,
        decimal missingAccessories,
        decimal targetMargin = 0.15m)
    {
        var baseMax = expectedSale * (1m - targetMargin) - repair - cleaning - missingAccessories;
        if (baseMax < 0) baseMax = 0;
        // Floor to nearest 100 THB step
        return Math.Floor(baseMax / 100m) * 100m;
    }

    private static void RecalculateTotals(PurchaseOrder po)
    {
        po.ApprovedBy = null;
        po.ApprovedAt = null;
        po.TotalEstimatedValue = po.Items.Sum(i => i.ExpectedSalePrice);
        po.TotalMaxBuyPrice = po.Items.Sum(i => i.MaxBuyPrice);
        po.TotalOfferedPrice = po.Items.Sum(i => i.OfferedPrice);
        po.TotalAgreedPrice = po.Items.Sum(i => i.AgreedPrice);
        po.NeedsApproval = po.TotalOfferedPrice > po.TotalMaxBuyPrice;
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder po)
    {
        var items = po.Items.Select(i => new PurchaseItemDto(
            i.Id,
            i.PurchaseOrderId,
            i.ProductId,
            i.Product?.ModelName ?? "",
            i.Product?.ProductCode ?? "",
            i.Product?.Brand?.Name ?? "",
            i.Product?.Category?.Name ?? "",
            i.Product?.Category?.Type ?? CategoryType.Other,
            i.SerialNumber,
            i.ConditionGradeId,
            i.ConditionGrade?.GradeCode ?? "",
            i.ConditionGrade?.GradeName ?? "",
            i.ShutterCount,
            i.Accessories,
            i.Defects,
            i.ExpectedSalePrice,
            i.EstimatedRepairCost,
            i.EstimatedCleaningCost,
            i.MissingAccessoriesDeduction,
            i.MaxBuyPrice,
            i.OfferedPrice,
            i.AgreedPrice,
            i.InventoryItemId,
            i.InventoryItem?.Sku,
            i.InspectionResults.Select(ir => new InspectionResultDto(
                ir.Id,
                ir.InspectionTemplateItemId,
                ir.InspectionTemplateItem?.CategoryGroup ?? "",
                ir.InspectionTemplateItem?.Title ?? "",
                ir.InspectionTemplateItem?.IsCritical ?? false,
                ir.Result,
                FormatInspectionResult(ir.Result),
                ir.Notes,
                ir.PhotoUrl
            )).ToList()
        )).ToList();

        var payments = po.Payments.Select(p => new PurchasePaymentDto(
            p.Id,
            p.PaymentMethod,
            FormatPaymentMethod(p.PaymentMethod),
            p.Amount,
            p.PaidAt,
            p.ReferenceNumber,
            p.ProofUrl,
            p.Notes
        )).ToList();

        return new PurchaseOrderDto(
            po.Id,
            po.PoNumber,
            po.CustomerId,
            po.Customer?.Name ?? "",
            po.Customer?.Phone ?? "",
            po.Customer?.IdCardNumber,
            po.Status,
            FormatPurchaseStatus(po.Status),
            po.TotalEstimatedValue,
            po.TotalMaxBuyPrice,
            po.TotalOfferedPrice,
            po.TotalAgreedPrice,
            po.NeedsApproval,
            po.ApprovedBy,
            po.ApprovedAt,
            po.Notes,
            po.PurchaseDate,
            items,
            payments,
            po.CreatedAt
        );
    }

    private async Task<InspectionTemplate> SeedDefaultTemplateIfMissingAsync(Guid storeId, CategoryType categoryType)
    {
        var template = new InspectionTemplate
        {
            StoreId = storeId,
            CategoryType = categoryType,
            Name = categoryType == CategoryType.Camera ? "เกณฑ์ตรวจเช็คสภาพตัวกล้อง (Camera Body Checklist)" : "เกณฑ์ตรวจเช็คสภาพเลนส์ (Lens Checklist)",
            Description = "เกณฑ์มาตรฐานประเมินสภาพกล้องและเลนส์มือสอง",
            IsActive = true
        };

        if (categoryType == CategoryType.Camera)
        {
            template.Items = new List<InspectionTemplateItem>
            {
                new() { CategoryGroup = "เซนเซอร์และภาพถ่าย", Title = "ตรวจเช็คฝุ่น รอยขีดข่วน และ Dead/Hot Pixel บนเซนเซอร์", IsCritical = true, SortOrder = 1 },
                new() { CategoryGroup = "ระบบชัตเตอร์", Title = "ทดสอบการลั่นชัตเตอร์ทุกความเร็วสปีด (รวมทั้งสปีดสูงและชัตเตอร์ B)", IsCritical = true, SortOrder = 2 },
                new() { CategoryGroup = "ระบบโฟกัสและชัตเตอร์", Title = "ทดสอบระบบ Autofocus, Tracking และ Eye-AF", IsCritical = true, SortOrder = 3 },
                new() { CategoryGroup = "ช่องมองภาพและจอ LCD", Title = "ตรวจเช็คหน้าจอ LCD ไม่ด่าง ไม่ลอก ทัชสกรีนทำงานปกติ และ EVF ชัดเจน", IsCritical = false, SortOrder = 4 },
                new() { CategoryGroup = "สภาพภายนอกและบอดี้", Title = "ตรวจเช็คริ้วรอยขนแมว รอยบุบ ยางบวม หรือรอยตกกระแทก", IsCritical = false, SortOrder = 5 },
                new() { CategoryGroup = "ระบบไฟฟ้าและพอร์ต", Title = "ทดสอบช่องใส่ SD Card, พอร์ตชาร์จ USB, HDMI และช่องใส่แบตเตอรี่", IsCritical = true, SortOrder = 6 }
            };
        }
        else
        {
            template.Items = new List<InspectionTemplateItem>
            {
                new() { CategoryGroup = "ชิ้นเลนส์และโค้ทติ้ง", Title = "ส่องไฟเช็ค รา ฝ้า รอยขนแมวที่ชิ้นเลนส์หน้า-หลัง", IsCritical = true, SortOrder = 1 },
                new() { CategoryGroup = "ระบบมอเตอร์และโฟกัส", Title = "ทดสอบระบบ Autofocus เร็ว เงียบ แม่นยำ ไม่ติดขัด", IsCritical = true, SortOrder = 2 },
                new() { CategoryGroup = "ระบบรูรับแสง (Aperture)", Title = "ทดสอบใบเบลดรูรับแสง เปิด-หรี่ทุก F-stop ไม่มีคราบน้ำมัน", IsCritical = true, SortOrder = 3 },
                new() { CategoryGroup = "วงแหวนและกระบอกเลนส์", Title = "วงแหวนซูมและวงแหวนโฟกัส หมุนลื่น สมูท ไม่หลวมคลอน", IsCritical = false, SortOrder = 4 },
                new() { CategoryGroup = "ระบบกันสั่น (IS/VR/OSS)", Title = "ทดสอบระบบกันสั่นในตัวเลนส์ทำงานเงียบปกติ", IsCritical = true, SortOrder = 5 }
            };
        }

        _context.InspectionTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    private static string GetCategoryPrefix(CategoryType type) => type switch
    {
        CategoryType.Camera => "CAM",
        CategoryType.Lens => "LEN",
        CategoryType.Flash => "FLA",
        CategoryType.Accessory => "ACC",
        _ => "ITM"
    };

    private static string FormatCategoryType(CategoryType type) => type switch
    {
        CategoryType.Camera => "กล้องถ่ายภาพ (Camera)",
        CategoryType.Lens => "เลนส์ (Lens)",
        CategoryType.Flash => "แฟลช (Flash)",
        CategoryType.Accessory => "อุปกรณ์เสริม (Accessory)",
        _ => "อื่นๆ"
    };

    private static string FormatPurchaseStatus(PurchaseStatus status) => status switch
    {
        PurchaseStatus.Draft => "ร่างรายการ",
        PurchaseStatus.Appraising => "กำลังตรวจสภาพและประเมินราคา",
        PurchaseStatus.Offered => "เสนอราคาแล้ว",
        PurchaseStatus.Accepted => "ลูกค้าตกลงรับข้อเสนอ",
        PurchaseStatus.Rejected => "ลูกค้ายกเลิก/ปฏิเสธข้อเสนอ",
        PurchaseStatus.Paid => "ชำระเงินเรียบร้อยแล้ว",
        PurchaseStatus.Received => "นำเข้าคลังสินค้าแล้ว",
        PurchaseStatus.Cancelled => "ยกเลิกใบรับซื้อ",
        _ => status.ToString()
    };

    private static string FormatInspectionResult(InspectionItemResult result) => result switch
    {
        InspectionItemResult.Pass => "ผ่าน (Pass)",
        InspectionItemResult.MinorIssue => "มีตำหนิเล็กน้อย (Minor Issue)",
        InspectionItemResult.Fail => "ไม่ผ่าน (Fail)",
        InspectionItemResult.NotApplicable => "ไม่เกี่ยวข้อง (N/A)",
        _ => result.ToString()
    };

    private static string FormatPaymentMethod(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "เงินสด",
        PaymentMethod.BankTransfer => "โอนเงินผ่านธนาคาร",
        PaymentMethod.PromptPay => "พร้อมเพย์",
        PaymentMethod.CreditCard => "บัตรเครดิต",
        _ => "อื่นๆ"
    };
    private static void EnsureUniqueItem(PurchaseOrder po, PurchaseItem item)
    {
        if (po.Items.Any(i => i.ProductId == item.ProductId &&
            string.Equals(i.SerialNumber, item.SerialNumber, StringComparison.OrdinalIgnoreCase)))
            throw AppException.Conflict($"มี Serial Number '{item.SerialNumber}' ซ้ำในใบรับซื้อ");
    }

    private static void EnsureEditable(PurchaseOrder po)
    {
        if (po.Status != PurchaseStatus.Draft && po.Status != PurchaseStatus.Appraising && po.Status != PurchaseStatus.Offered)
            throw AppException.BadRequest("แก้ไขรายการได้เฉพาะใบรับซื้อที่ยังไม่ตกลงรับซื้อเท่านั้น");
    }
}
