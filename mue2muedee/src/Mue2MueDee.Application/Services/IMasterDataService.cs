using Mue2MueDee.Application.DTOs;

namespace Mue2MueDee.Application.Services;

public interface IMasterDataService
{
    // Brands
    Task<IReadOnlyList<BrandDto>> GetBrandsAsync();
    Task<BrandDto> GetBrandByIdAsync(Guid id);
    Task<BrandDto> CreateBrandAsync(CreateBrandRequest request);
    Task<BrandDto> UpdateBrandAsync(Guid id, UpdateBrandRequest request);
    Task DeleteBrandAsync(Guid id);

    // Categories
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto> GetCategoryByIdAsync(Guid id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task DeleteCategoryAsync(Guid id);

    // ConditionGrades
    Task<IReadOnlyList<ConditionGradeDto>> GetConditionGradesAsync();
    Task<ConditionGradeDto> GetConditionGradeByIdAsync(Guid id);
    Task<ConditionGradeDto> CreateConditionGradeAsync(CreateConditionGradeRequest request);
    Task<ConditionGradeDto> UpdateConditionGradeAsync(Guid id, UpdateConditionGradeRequest request);
    Task DeleteConditionGradeAsync(Guid id);

    // Customers
    Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(string? search = null);
    Task<CustomerDto> GetCustomerByIdAsync(Guid id);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request);
    Task DeleteCustomerAsync(Guid id);

    // Products
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(Guid? brandId = null, Guid? categoryId = null, string? search = null);
    Task<ProductDto> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task DeleteProductAsync(Guid id);
}
