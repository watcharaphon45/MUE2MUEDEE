using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public class MasterDataService : IMasterDataService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentStore _currentStore;

    public MasterDataService(IApplicationDbContext context, ICurrentStore currentStore)
    {
        _context = context;
        _currentStore = currentStore;
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

    // ==========================================
    // BRANDS
    // ==========================================

    public async Task<IReadOnlyList<BrandDto>> GetBrandsAsync()
    {
        var storeId = RequireStoreId();
        var brands = await _context.Brands
            .Where(b => b.StoreId == storeId)
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto(
                b.Id,
                b.Name,
                b.Description,
                b.LogoUrl,
                b.IsActive,
                b.Products.Count,
                b.CreatedAt
            ))
            .ToListAsync();

        return brands;
    }

    public async Task<BrandDto> GetBrandByIdAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var brand = await _context.Brands
            .Where(b => b.StoreId == storeId && b.Id == id)
            .Select(b => new BrandDto(
                b.Id,
                b.Name,
                b.Description,
                b.LogoUrl,
                b.IsActive,
                b.Products.Count,
                b.CreatedAt
            ))
            .FirstOrDefaultAsync();

        if (brand == null)
            throw AppException.NotFound($"ไม่พบข้อมูลแบรนด์รหัส {id}");

        return brand;
    }

    public async Task<BrandDto> CreateBrandAsync(CreateBrandRequest request)
    {
        var storeId = RequireStoreId();
        var normalizedName = request.Name.Trim();

        var exists = await _context.Brands
            .AnyAsync(b => b.StoreId == storeId && b.Name.ToLower() == normalizedName.ToLower());

        if (exists)
            throw AppException.Conflict($"แบรนด์ '{normalizedName}' มีอยู่ในระบบแล้ว");

        var brand = new Brand
        {
            StoreId = storeId,
            Name = normalizedName,
            Description = request.Description?.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            IsActive = true
        };

        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        return new BrandDto(brand.Id, brand.Name, brand.Description, brand.LogoUrl, brand.IsActive, 0, brand.CreatedAt);
    }

    public async Task<BrandDto> UpdateBrandAsync(Guid id, UpdateBrandRequest request)
    {
        var storeId = RequireStoreId();
        var brand = await _context.Brands
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.StoreId == storeId && b.Id == id);

        if (brand == null)
            throw AppException.NotFound($"ไม่พบข้อมูลแบรนด์ที่ต้องการแก้ไข (รหัส: {id})");

        var normalizedName = request.Name.Trim();
        var duplicate = await _context.Brands
            .AnyAsync(b => b.StoreId == storeId && b.Id != id && b.Name.ToLower() == normalizedName.ToLower());

        if (duplicate)
            throw AppException.Conflict($"มีแบรนด์ชื่อ '{normalizedName}' อยู่ในระบบแล้ว");

        brand.Name = normalizedName;
        brand.Description = request.Description?.Trim();
        brand.LogoUrl = request.LogoUrl?.Trim();
        brand.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new BrandDto(brand.Id, brand.Name, brand.Description, brand.LogoUrl, brand.IsActive, brand.Products.Count, brand.CreatedAt);
    }

    public async Task DeleteBrandAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var brand = await _context.Brands
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.StoreId == storeId && b.Id == id);

        if (brand == null)
            throw AppException.NotFound($"ไม่พบแบรนด์รหัส {id}");

        if (brand.Products.Count > 0)
            throw AppException.BadRequest($"ไม่สามารถลบแบรนด์ '{brand.Name}' ได้ เนื่องจากมีรุ่นสินค้าเชื่อมโยงอยู่ {brand.Products.Count} รายการ");

        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // CATEGORIES
    // ==========================================

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
    {
        var storeId = RequireStoreId();
        var categories = await _context.Categories
            .Where(c => c.StoreId == storeId)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Type,
                FormatCategoryType(c.Type),
                c.Description,
                c.IsActive,
                c.Products.Count,
                c.CreatedAt
            ))
            .ToListAsync();

        return categories;
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var cat = await _context.Categories
            .Where(c => c.StoreId == storeId && c.Id == id)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Type,
                FormatCategoryType(c.Type),
                c.Description,
                c.IsActive,
                c.Products.Count,
                c.CreatedAt
            ))
            .FirstOrDefaultAsync();

        if (cat == null)
            throw AppException.NotFound($"ไม่พบหมวดหมู่รหัส {id}");

        return cat;
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var storeId = RequireStoreId();
        var normalizedName = request.Name.Trim();

        var exists = await _context.Categories
            .AnyAsync(c => c.StoreId == storeId && c.Name.ToLower() == normalizedName.ToLower());

        if (exists)
            throw AppException.Conflict($"หมวดหมู่ '{normalizedName}' มีอยู่ในระบบแล้ว");

        var category = new Category
        {
            StoreId = storeId,
            Name = normalizedName,
            Type = request.Type,
            Description = request.Description?.Trim(),
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, category.Type, FormatCategoryType(category.Type), category.Description, category.IsActive, 0, category.CreatedAt);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var storeId = RequireStoreId();
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.StoreId == storeId && c.Id == id);

        if (category == null)
            throw AppException.NotFound($"ไม่พบหมวดหมู่รหัส {id}");

        var normalizedName = request.Name.Trim();
        var duplicate = await _context.Categories
            .AnyAsync(c => c.StoreId == storeId && c.Id != id && c.Name.ToLower() == normalizedName.ToLower());

        if (duplicate)
            throw AppException.Conflict($"มีหมวดหมู่ชื่อ '{normalizedName}' อยู่ในระบบแล้ว");

        category.Name = normalizedName;
        category.Type = request.Type;
        category.Description = request.Description?.Trim();
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, category.Type, FormatCategoryType(category.Type), category.Description, category.IsActive, category.Products.Count, category.CreatedAt);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.StoreId == storeId && c.Id == id);

        if (category == null)
            throw AppException.NotFound($"ไม่พบหมวดหมู่รหัส {id}");

        if (category.Products.Count > 0)
            throw AppException.BadRequest($"ไม่สามารถลบหมวดหมู่ '{category.Name}' ได้ เนื่องจากมีรุ่นสินค้าผูกอยู่ {category.Products.Count} รายการ");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // CONDITION GRADES
    // ==========================================

    public async Task<IReadOnlyList<ConditionGradeDto>> GetConditionGradesAsync()
    {
        var storeId = RequireStoreId();
        var grades = await _context.ConditionGrades
            .Where(cg => cg.StoreId == storeId)
            .OrderBy(cg => cg.SortOrder)
            .Select(cg => new ConditionGradeDto(
                cg.Id,
                cg.GradeCode,
                cg.GradeName,
                cg.Description,
                cg.SortOrder,
                cg.IsActive
            ))
            .ToListAsync();

        return grades;
    }

    public async Task<ConditionGradeDto> GetConditionGradeByIdAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var grade = await _context.ConditionGrades
            .Where(cg => cg.StoreId == storeId && cg.Id == id)
            .Select(cg => new ConditionGradeDto(
                cg.Id,
                cg.GradeCode,
                cg.GradeName,
                cg.Description,
                cg.SortOrder,
                cg.IsActive
            ))
            .FirstOrDefaultAsync();

        if (grade == null)
            throw AppException.NotFound($"ไม่พบข้อมูลเกรดสภาพรหัส {id}");

        return grade;
    }

    public async Task<ConditionGradeDto> CreateConditionGradeAsync(CreateConditionGradeRequest request)
    {
        var storeId = RequireStoreId();
        var code = request.GradeCode.Trim().ToUpperInvariant();

        var exists = await _context.ConditionGrades
            .AnyAsync(cg => cg.StoreId == storeId && cg.GradeCode == code);

        if (exists)
            throw AppException.Conflict($"รหัสเกรด '{code}' มีอยู่ในระบบแล้ว");

        var grade = new ConditionGrade
        {
            StoreId = storeId,
            GradeCode = code,
            GradeName = request.GradeName.Trim(),
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder,
            IsActive = true
        };

        _context.ConditionGrades.Add(grade);
        await _context.SaveChangesAsync();

        return new ConditionGradeDto(grade.Id, grade.GradeCode, grade.GradeName, grade.Description, grade.SortOrder, grade.IsActive);
    }

    public async Task<ConditionGradeDto> UpdateConditionGradeAsync(Guid id, UpdateConditionGradeRequest request)
    {
        var storeId = RequireStoreId();
        var grade = await _context.ConditionGrades
            .FirstOrDefaultAsync(cg => cg.StoreId == storeId && cg.Id == id);

        if (grade == null)
            throw AppException.NotFound($"ไม่พบข้อมูลเกรดสภาพรหัส {id}");

        var code = request.GradeCode.Trim().ToUpperInvariant();
        var duplicate = await _context.ConditionGrades
            .AnyAsync(cg => cg.StoreId == storeId && cg.Id != id && cg.GradeCode == code);

        if (duplicate)
            throw AppException.Conflict($"มีรหัสเกรด '{code}' อยู่ในระบบแล้ว");

        grade.GradeCode = code;
        grade.GradeName = request.GradeName.Trim();
        grade.Description = request.Description?.Trim();
        grade.SortOrder = request.SortOrder;
        grade.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new ConditionGradeDto(grade.Id, grade.GradeCode, grade.GradeName, grade.Description, grade.SortOrder, grade.IsActive);
    }

    public async Task DeleteConditionGradeAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var grade = await _context.ConditionGrades
            .Include(cg => cg.InventoryItems)
            .FirstOrDefaultAsync(cg => cg.StoreId == storeId && cg.Id == id);

        if (grade == null)
            throw AppException.NotFound($"ไม่พบเกรดสภาพรหัส {id}");

        if (grade.InventoryItems.Count > 0)
            throw AppException.BadRequest($"ไม่สามารถลบเกรดสภาพ '{grade.GradeCode}' ได้ เนื่องจากมีสินค้าในคลังที่ใช้เกรดนี้อยู่ {grade.InventoryItems.Count} ชิ้น");

        _context.ConditionGrades.Remove(grade);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // CUSTOMERS
    // ==========================================

    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(string? search = null)
    {
        var storeId = RequireStoreId();
        var query = _context.Customers
            .Where(c => c.StoreId == storeId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(s) ||
                c.Phone.Contains(s) ||
                (c.IdCardNumber != null && c.IdCardNumber.Contains(s)) ||
                (c.Email != null && c.Email.ToLower().Contains(s)));
        }

        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto(
                c.Id,
                c.CustomerType,
                FormatCustomerType(c.CustomerType),
                c.Name,
                c.Phone,
                c.Email,
                c.IdCardNumber,
                c.Address,
                c.LineId,
                c.Facebook,
                c.BankName,
                c.BankAccountNumber,
                c.BankAccountName,
                c.Notes,
                c.IsActive,
                c.SourcedInventoryItems.Count,
                c.CreatedAt
            ))
            .ToListAsync();

        return customers;
    }

    public async Task<CustomerDto> GetCustomerByIdAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var customer = await _context.Customers
            .Where(c => c.StoreId == storeId && c.Id == id)
            .Select(c => new CustomerDto(
                c.Id,
                c.CustomerType,
                FormatCustomerType(c.CustomerType),
                c.Name,
                c.Phone,
                c.Email,
                c.IdCardNumber,
                c.Address,
                c.LineId,
                c.Facebook,
                c.BankName,
                c.BankAccountNumber,
                c.BankAccountName,
                c.Notes,
                c.IsActive,
                c.SourcedInventoryItems.Count,
                c.CreatedAt
            ))
            .FirstOrDefaultAsync();

        if (customer == null)
            throw AppException.NotFound($"ไม่พบข้อมูลลูกค้ารหัส {id}");

        return customer;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var storeId = RequireStoreId();
        var phone = request.Phone.Trim();

        var customer = new Customer
        {
            StoreId = storeId,
            CustomerType = request.CustomerType,
            Name = request.Name.Trim(),
            Phone = phone,
            Email = request.Email?.Trim(),
            IdCardNumber = request.IdCardNumber?.Trim(),
            Address = request.Address?.Trim(),
            LineId = request.LineId?.Trim(),
            Facebook = request.Facebook?.Trim(),
            BankName = request.BankName?.Trim(),
            BankAccountNumber = request.BankAccountNumber?.Trim(),
            BankAccountName = request.BankAccountName?.Trim(),
            Notes = request.Notes?.Trim(),
            IsActive = true
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerDto(
            customer.Id,
            customer.CustomerType,
            FormatCustomerType(customer.CustomerType),
            customer.Name,
            customer.Phone,
            customer.Email,
            customer.IdCardNumber,
            customer.Address,
            customer.LineId,
            customer.Facebook,
            customer.BankName,
            customer.BankAccountNumber,
            customer.BankAccountName,
            customer.Notes,
            customer.IsActive,
            0,
            customer.CreatedAt
        );
    }

    public async Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request)
    {
        var storeId = RequireStoreId();
        var customer = await _context.Customers
            .Include(c => c.SourcedInventoryItems)
            .FirstOrDefaultAsync(c => c.StoreId == storeId && c.Id == id);

        if (customer == null)
            throw AppException.NotFound($"ไม่พบข้อมูลลูกค้ารหัส {id}");

        customer.CustomerType = request.CustomerType;
        customer.Name = request.Name.Trim();
        customer.Phone = request.Phone.Trim();
        customer.Email = request.Email?.Trim();
        customer.IdCardNumber = request.IdCardNumber?.Trim();
        customer.Address = request.Address?.Trim();
        customer.LineId = request.LineId?.Trim();
        customer.Facebook = request.Facebook?.Trim();
        customer.BankName = request.BankName?.Trim();
        customer.BankAccountNumber = request.BankAccountNumber?.Trim();
        customer.BankAccountName = request.BankAccountName?.Trim();
        customer.Notes = request.Notes?.Trim();
        customer.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new CustomerDto(
            customer.Id,
            customer.CustomerType,
            FormatCustomerType(customer.CustomerType),
            customer.Name,
            customer.Phone,
            customer.Email,
            customer.IdCardNumber,
            customer.Address,
            customer.LineId,
            customer.Facebook,
            customer.BankName,
            customer.BankAccountNumber,
            customer.BankAccountName,
            customer.Notes,
            customer.IsActive,
            customer.SourcedInventoryItems.Count,
            customer.CreatedAt
        );
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var customer = await _context.Customers
            .Include(c => c.SourcedInventoryItems)
            .FirstOrDefaultAsync(c => c.StoreId == storeId && c.Id == id);

        if (customer == null)
            throw AppException.NotFound($"ไม่พบลูกค้ารหัส {id}");

        if (customer.SourcedInventoryItems.Count > 0)
            throw AppException.BadRequest($"ไม่สามารถลบลูกค้า '{customer.Name}' ได้ เนื่องจากมีประวัติการรับซื้อสินค้าเข้าคลัง {customer.SourcedInventoryItems.Count} รายการ");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // PRODUCTS
    // ==========================================

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(Guid? brandId = null, Guid? categoryId = null, string? search = null)
    {
        var storeId = RequireStoreId();
        var query = _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Where(p => p.StoreId == storeId);

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p =>
                p.ModelName.ToLower().Contains(s) ||
                p.ProductCode.ToLower().Contains(s) ||
                p.Brand.Name.ToLower().Contains(s) ||
                p.Category.Name.ToLower().Contains(s));
        }

        var products = await query
            .OrderBy(p => p.Brand.Name)
            .ThenBy(p => p.ModelName)
            .Select(p => new ProductDto(
                p.Id,
                p.BrandId,
                p.Brand.Name,
                p.CategoryId,
                p.Category.Name,
                p.ModelName,
                p.ProductCode,
                p.Description,
                p.DefaultSellingPrice,
                p.ImageUrl,
                p.IsActive,
                p.InventoryItems.Count(i => i.Status == InventoryStatus.Available),
                p.CreatedAt
            ))
            .ToListAsync();

        return products;
    }

    public async Task<ProductDto> GetProductByIdAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Where(p => p.StoreId == storeId && p.Id == id)
            .Select(p => new ProductDto(
                p.Id,
                p.BrandId,
                p.Brand.Name,
                p.CategoryId,
                p.Category.Name,
                p.ModelName,
                p.ProductCode,
                p.Description,
                p.DefaultSellingPrice,
                p.ImageUrl,
                p.IsActive,
                p.InventoryItems.Count(i => i.Status == InventoryStatus.Available),
                p.CreatedAt
            ))
            .FirstOrDefaultAsync();

        if (product == null)
            throw AppException.NotFound($"ไม่พบข้อมูลสินค้ามาสเตอร์รหัส {id}");

        return product;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        var storeId = RequireStoreId();

        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.StoreId == storeId && b.Id == request.BrandId);
        if (brand == null)
            throw AppException.NotFound($"ไม่พบแบรนด์รหัส {request.BrandId}");

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.StoreId == storeId && c.Id == request.CategoryId);
        if (category == null)
            throw AppException.NotFound($"ไม่พบหมวดหมู่รหัส {request.CategoryId}");

        var code = request.ProductCode.Trim().ToUpperInvariant();
        var codeExists = await _context.Products.AnyAsync(p => p.StoreId == storeId && p.ProductCode == code);
        if (codeExists)
            throw AppException.Conflict($"รหัสสินค้า '{code}' มีอยู่ในระบบแล้ว");

        var product = new Product
        {
            StoreId = storeId,
            BrandId = request.BrandId,
            CategoryId = request.CategoryId,
            ModelName = request.ModelName.Trim(),
            ProductCode = code,
            Description = request.Description?.Trim(),
            DefaultSellingPrice = request.DefaultSellingPrice,
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductDto(
            product.Id,
            brand.Id,
            brand.Name,
            category.Id,
            category.Name,
            product.ModelName,
            product.ProductCode,
            product.Description,
            product.DefaultSellingPrice,
            product.ImageUrl,
            product.IsActive,
            0,
            product.CreatedAt
        );
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var storeId = RequireStoreId();
        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.InventoryItems)
            .FirstOrDefaultAsync(p => p.StoreId == storeId && p.Id == id);

        if (product == null)
            throw AppException.NotFound($"ไม่พบสินค้ามาสเตอร์รหัส {id}");

        if (product.BrandId != request.BrandId)
        {
            var brandExists = await _context.Brands.AnyAsync(b => b.StoreId == storeId && b.Id == request.BrandId);
            if (!brandExists)
                throw AppException.NotFound($"ไม่พบแบรนด์รหัส {request.BrandId}");
            product.BrandId = request.BrandId;
        }

        if (product.CategoryId != request.CategoryId)
        {
            var catExists = await _context.Categories.AnyAsync(c => c.StoreId == storeId && c.Id == request.CategoryId);
            if (!catExists)
                throw AppException.NotFound($"ไม่พบหมวดหมู่รหัส {request.CategoryId}");
            product.CategoryId = request.CategoryId;
        }

        var code = request.ProductCode.Trim().ToUpperInvariant();
        var codeDuplicate = await _context.Products.AnyAsync(p => p.StoreId == storeId && p.Id != id && p.ProductCode == code);
        if (codeDuplicate)
            throw AppException.Conflict($"รหัสสินค้า '{code}' มีอยู่ในระบบแล้ว");

        product.ModelName = request.ModelName.Trim();
        product.ProductCode = code;
        product.Description = request.Description?.Trim();
        product.DefaultSellingPrice = request.DefaultSellingPrice;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        // Reload brand & category names
        var brand = await _context.Brands.FirstAsync(b => b.Id == product.BrandId);
        var category = await _context.Categories.FirstAsync(c => c.Id == product.CategoryId);

        return new ProductDto(
            product.Id,
            brand.Id,
            brand.Name,
            category.Id,
            category.Name,
            product.ModelName,
            product.ProductCode,
            product.Description,
            product.DefaultSellingPrice,
            product.ImageUrl,
            product.IsActive,
            product.InventoryItems.Count(i => i.Status == InventoryStatus.Available),
            product.CreatedAt
        );
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var storeId = RequireStoreId();
        var product = await _context.Products
            .Include(p => p.InventoryItems)
            .FirstOrDefaultAsync(p => p.StoreId == storeId && p.Id == id);

        if (product == null)
            throw AppException.NotFound($"ไม่พบสินค้ามาสเตอร์รหัส {id}");

        if (product.InventoryItems.Count > 0)
            throw AppException.BadRequest($"ไม่สามารถลบรุ่นสินค้า '{product.ModelName}' ได้ เนื่องจากมีรายการสต็อกสินค้าเชื่อมโยงอยู่ {product.InventoryItems.Count} รายการ");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    // ==========================================
    // Helpers
    // ==========================================

    private static string FormatCategoryType(CategoryType type) => type switch
    {
        CategoryType.Camera => "กล้องถ่ายภาพ (Camera)",
        CategoryType.Lens => "เลนส์ (Lens)",
        CategoryType.Flash => "แฟลช (Flash)",
        CategoryType.Accessory => "อุปกรณ์เสริม (Accessory)",
        _ => "อื่นๆ (Other)"
    };

    private static string FormatCustomerType(CustomerType type) => type switch
    {
        CustomerType.Seller => "ผู้ขายของเข้า (Seller)",
        CustomerType.Buyer => "ลูกค้าซื้อ (Buyer)",
        CustomerType.Both => "ซื้อและขาย (Both)",
        _ => "ทั่วไป"
    };
}
