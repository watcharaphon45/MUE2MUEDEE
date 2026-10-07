using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public interface ICatalogService
{
    Task<PublicStoreInfoDto> GetStoreInfoByCodeAsync(string storeCode, CancellationToken ct = default);
    Task<PublicCatalogResponse> GetCatalogByStoreCodeAsync(string storeCode, CatalogQueryParameters parameters, CancellationToken ct = default);
    Task<PublicCatalogItemDto> GetCatalogItemDetailAsync(string storeCode, Guid itemId, CancellationToken ct = default);
}

public class CatalogService : ICatalogService
{
    private readonly IApplicationDbContext _context;

    public CatalogService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PublicStoreInfoDto> GetStoreInfoByCodeAsync(string storeCode, CancellationToken ct = default)
    {
        var normalizedCode = storeCode.Trim().ToUpper();

        var store = await _context.Stores
            .AsNoTracking()
            .Include(s => s.UserStores)
                .ThenInclude(us => us.User)
            .FirstOrDefaultAsync(s => s.Code.ToUpper() == normalizedCode && s.IsActive, ct);

        if (store == null)
        {
            throw new KeyNotFoundException($"ไม่พบร้านค้ารหัส '{storeCode}' หรือร้านค้านี้ยังไม่เปิดให้บริการ");
        }

        return MapStoreToPublicDto(store);
    }

    public async Task<PublicCatalogResponse> GetCatalogByStoreCodeAsync(string storeCode, CatalogQueryParameters parameters, CancellationToken ct = default)
    {
        var normalizedCode = storeCode.Trim().ToUpper();

        var store = await _context.Stores
            .AsNoTracking()
            .Include(s => s.UserStores)
                .ThenInclude(us => us.User)
            .FirstOrDefaultAsync(s => s.Code.ToUpper() == normalizedCode && s.IsActive, ct);

        if (store == null)
        {
            throw new KeyNotFoundException($"ไม่พบร้านค้ารหัส '{storeCode}' หรือร้านค้านี้ยังไม่เปิดให้บริการ");
        }

        var storeDto = MapStoreToPublicDto(store);

        // Base query for available inventory items in this store (ignoring global tenant filter so anyone can browse public catalog)
        var query = _context.InventoryItems
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(i => i.Product)
                .ThenInclude(p => p.Brand)
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .Where(i => !i.IsDeleted && i.StoreId == store.Id && i.Status == InventoryStatus.Available);

        // Fetch filter metadata before applying individual filters (so customer sees available brands/categories)
        var allAvailableItems = await query.Select(i => new
        {
            BrandId = i.Product.BrandId,
            BrandName = i.Product.Brand.Name,
            CategoryId = i.Product.CategoryId,
            CategoryName = i.Product.Category.Name,
            GradeId = i.ConditionGradeId,
            GradeCode = i.ConditionGrade.GradeCode,
            GradeName = i.ConditionGrade.GradeName
        }).ToListAsync(ct);

        var filterMeta = new PublicCatalogFilterMetaDto(
            Brands: allAvailableItems
                .Select(x => new CatalogFilterOptionDto(x.BrandId, x.BrandName))
                .DistinctBy(x => x.Id)
                .OrderBy(x => x.Name)
                .ToList(),
            Categories: allAvailableItems
                .Select(x => new CatalogFilterOptionDto(x.CategoryId, x.CategoryName))
                .DistinctBy(x => x.Id)
                .OrderBy(x => x.Name)
                .ToList(),
            ConditionGrades: allAvailableItems
                .Select(x => new CatalogFilterOptionDto(x.GradeId, x.GradeName, x.GradeCode))
                .DistinctBy(x => x.Id)
                .OrderBy(x => x.Code)
                .ToList(),
            TotalAvailableItems: allAvailableItems.Count
        );

        // Apply filters
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchLower = parameters.Search.Trim().ToLower();
            query = query.Where(i =>
                i.Product.ModelName.ToLower().Contains(searchLower) ||
                i.Product.Brand.Name.ToLower().Contains(searchLower) ||
                i.Product.ProductCode.ToLower().Contains(searchLower) ||
                i.Sku.ToLower().Contains(searchLower)
            );
        }

        if (parameters.BrandId.HasValue)
        {
            query = query.Where(i => i.Product.BrandId == parameters.BrandId.Value);
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(i => i.Product.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.ConditionGradeId.HasValue)
        {
            query = query.Where(i => i.ConditionGradeId == parameters.ConditionGradeId.Value);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(i => i.SellingPrice >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(i => i.SellingPrice <= parameters.MaxPrice.Value);
        }

        // Apply sorting
        query = parameters.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(i => i.SellingPrice),
            "price_desc" => query.OrderByDescending(i => i.SellingPrice),
            "model" => query.OrderBy(i => i.Product.ModelName),
            _ => query.OrderByDescending(i => i.ReceivedAt).ThenByDescending(i => i.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var page = Math.Max(1, parameters.Page);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new PublicCatalogItemDto(
                i.Id,
                i.Sku,
                i.Product.ModelName,
                i.Product.Brand.Name,
                i.Product.Category.Name,
                i.ConditionGrade.GradeName,
                i.ConditionGrade.GradeCode,
                i.SellingPrice,
                i.ShutterCount,
                i.Accessories,
                i.Defects,
                i.Notes,
                i.Product.ImageUrl,
                i.ReceivedAt
            ))
            .ToListAsync(ct);

        return new PublicCatalogResponse(
            Store: storeDto,
            Items: pagedItems,
            FilterMeta: filterMeta,
            TotalCount: totalCount,
            CurrentPage: page,
            TotalPages: totalPages
        );
    }

    public async Task<PublicCatalogItemDto> GetCatalogItemDetailAsync(string storeCode, Guid itemId, CancellationToken ct = default)
    {
        var normalizedCode = storeCode.Trim().ToUpper();

        var store = await _context.Stores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Code.ToUpper() == normalizedCode && s.IsActive, ct);

        if (store == null)
        {
            throw new KeyNotFoundException($"ไม่พบร้านค้ารหัส '{storeCode}'");
        }

        var item = await _context.InventoryItems
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(i => i.Product)
                .ThenInclude(p => p.Brand)
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Include(i => i.ConditionGrade)
            .FirstOrDefaultAsync(i =>
                i.Id == itemId &&
                i.StoreId == store.Id &&
                i.Status == InventoryStatus.Available &&
                !i.IsDeleted, ct);

        if (item == null)
        {
            throw new KeyNotFoundException("ไม่พบสินค้าที่ต้องการ หรือสินค้านี้ถูกจำหน่ายออกไปแล้ว");
        }

        return new PublicCatalogItemDto(
            item.Id,
            item.Sku,
            item.Product.ModelName,
            item.Product.Brand.Name,
            item.Product.Category.Name,
            item.ConditionGrade.GradeName,
            item.ConditionGrade.GradeCode,
            item.SellingPrice,
            item.ShutterCount,
            item.Accessories,
            item.Defects,
            item.Notes,
            item.Product.ImageUrl,
            item.ReceivedAt
        );
    }

    private static PublicStoreInfoDto MapStoreToPublicDto(Store store)
    {
        var ownerUs = store.UserStores.FirstOrDefault(us => us.Role == StoreRole.Owner && us.IsActive);
        var owner = ownerUs?.User;

        return new PublicStoreInfoDto(
            Id: store.Id,
            Name: store.Name,
            Code: store.Code,
            LogoUrl: store.LogoUrl,
            PrimaryColor: string.IsNullOrWhiteSpace(store.PrimaryColor) ? "#4f46e5" : store.PrimaryColor,
            Address: store.Address,
            Phone: !string.IsNullOrWhiteSpace(store.Phone) ? store.Phone : owner?.PhoneNumber,
            Email: !string.IsNullOrWhiteSpace(store.Email) ? store.Email : owner?.Email,
            Facebook: store.Facebook,
            Line: store.Line,
            Website: store.Website,
            OwnerName: owner?.FullName,
            OwnerPhone: owner?.PhoneNumber
        );
    }
}
