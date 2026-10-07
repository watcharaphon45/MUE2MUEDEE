using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.DTOs;

// --- Brand DTOs ---
public record BrandDto(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl,
    bool IsActive,
    int ProductCount,
    DateTimeOffset CreatedAt
);

public record CreateBrandRequest(
    string Name,
    string? Description,
    string? LogoUrl
);

public record UpdateBrandRequest(
    string Name,
    string? Description,
    string? LogoUrl,
    bool IsActive
);

// --- Category DTOs ---
public record CategoryDto(
    Guid Id,
    string Name,
    CategoryType Type,
    string TypeName,
    string? Description,
    bool IsActive,
    int ProductCount,
    DateTimeOffset CreatedAt
);

public record CreateCategoryRequest(
    string Name,
    CategoryType Type,
    string? Description
);

public record UpdateCategoryRequest(
    string Name,
    CategoryType Type,
    string? Description,
    bool IsActive
);

// --- ConditionGrade DTOs ---
public record ConditionGradeDto(
    Guid Id,
    string GradeCode,
    string GradeName,
    string? Description,
    int SortOrder,
    bool IsActive
);

public record CreateConditionGradeRequest(
    string GradeCode,
    string GradeName,
    string? Description,
    int SortOrder
);

public record UpdateConditionGradeRequest(
    string GradeCode,
    string GradeName,
    string? Description,
    int SortOrder,
    bool IsActive
);

// --- Customer DTOs ---
public record CustomerDto(
    Guid Id,
    CustomerType CustomerType,
    string CustomerTypeName,
    string Name,
    string Phone,
    string? Email,
    string? IdCardNumber,
    string? Address,
    string? LineId,
    string? Facebook,
    string? BankName,
    string? BankAccountNumber,
    string? BankAccountName,
    string? Notes,
    bool IsActive,
    int SourcedItemCount,
    DateTimeOffset CreatedAt
);

public record CreateCustomerRequest(
    CustomerType CustomerType,
    string Name,
    string Phone,
    string? Email,
    string? IdCardNumber,
    string? Address,
    string? LineId,
    string? Facebook,
    string? BankName,
    string? BankAccountNumber,
    string? BankAccountName,
    string? Notes
);

public record UpdateCustomerRequest(
    CustomerType CustomerType,
    string Name,
    string Phone,
    string? Email,
    string? IdCardNumber,
    string? Address,
    string? LineId,
    string? Facebook,
    string? BankName,
    string? BankAccountNumber,
    string? BankAccountName,
    string? Notes,
    bool IsActive
);

// --- Product DTOs ---
public record ProductDto(
    Guid Id,
    Guid BrandId,
    string BrandName,
    Guid CategoryId,
    string CategoryName,
    string ModelName,
    string ProductCode,
    string? Description,
    decimal? DefaultSellingPrice,
    string? ImageUrl,
    bool IsActive,
    int InStockCount,
    DateTimeOffset CreatedAt
);

public record CreateProductRequest(
    Guid BrandId,
    Guid CategoryId,
    string ModelName,
    string ProductCode,
    string? Description,
    decimal? DefaultSellingPrice,
    string? ImageUrl
);

public record UpdateProductRequest(
    Guid BrandId,
    Guid CategoryId,
    string ModelName,
    string ProductCode,
    string? Description,
    decimal? DefaultSellingPrice,
    string? ImageUrl,
    bool IsActive
);
