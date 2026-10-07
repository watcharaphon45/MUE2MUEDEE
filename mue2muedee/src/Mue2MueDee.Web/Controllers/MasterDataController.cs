using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Models;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[Authorize]
[Route("api/master-data")]
public class MasterDataController : ApiControllerBase
{
    private readonly IMasterDataService _masterDataService;

    public MasterDataController(IMasterDataService masterDataService)
    {
        _masterDataService = masterDataService;
    }

    // ==========================================
    // BRANDS
    // ==========================================

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands()
    {
        var brands = await _masterDataService.GetBrandsAsync();
        return Success(brands, "ดึงข้อมูลแบรนด์สำเร็จ");
    }

    [HttpGet("brands/{id:guid}")]
    public async Task<IActionResult> GetBrandById(Guid id)
    {
        var brand = await _masterDataService.GetBrandByIdAsync(id);
        return Success(brand, "ดึงข้อมูลแบรนด์สำเร็จ");
    }

    [HttpPost("brands")]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request)
    {
        var brand = await _masterDataService.CreateBrandAsync(request);
        return CreatedSuccess(brand, $"สร้างแบรนด์ '{brand.Name}' สำเร็จ");
    }

    [HttpPut("brands/{id:guid}")]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandRequest request)
    {
        var brand = await _masterDataService.UpdateBrandAsync(id, request);
        return Success(brand, $"แก้ไขข้อมูลแบรนด์ '{brand.Name}' สำเร็จ");
    }

    [HttpDelete("brands/{id:guid}")]
    public async Task<IActionResult> DeleteBrand(Guid id)
    {
        await _masterDataService.DeleteBrandAsync(id);
        return Success("ลบแบรนด์สำเร็จ");
    }

    // ==========================================
    // CATEGORIES
    // ==========================================

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _masterDataService.GetCategoriesAsync();
        return Success(categories, "ดึงข้อมูลหมวดหมู่สำเร็จ");
    }

    [HttpGet("categories/{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var category = await _masterDataService.GetCategoryByIdAsync(id);
        return Success(category, "ดึงข้อมูลหมวดหมู่สำเร็จ");
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = await _masterDataService.CreateCategoryAsync(request);
        return CreatedSuccess(category, $"สร้างหมวดหมู่ '{category.Name}' สำเร็จ");
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _masterDataService.UpdateCategoryAsync(id, request);
        return Success(category, $"แก้ไขหมวดหมู่ '{category.Name}' สำเร็จ");
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _masterDataService.DeleteCategoryAsync(id);
        return Success("ลบหมวดหมู่สำเร็จ");
    }

    // ==========================================
    // CONDITION GRADES
    // ==========================================

    [HttpGet("condition-grades")]
    public async Task<IActionResult> GetConditionGrades()
    {
        var grades = await _masterDataService.GetConditionGradesAsync();
        return Success(grades, "ดึงข้อมูลเกรดสภาพสินค้าสำเร็จ");
    }

    [HttpGet("condition-grades/{id:guid}")]
    public async Task<IActionResult> GetConditionGradeById(Guid id)
    {
        var grade = await _masterDataService.GetConditionGradeByIdAsync(id);
        return Success(grade, "ดึงข้อมูลเกรดสภาพสินค้าสำเร็จ");
    }

    [HttpPost("condition-grades")]
    public async Task<IActionResult> CreateConditionGrade([FromBody] CreateConditionGradeRequest request)
    {
        var grade = await _masterDataService.CreateConditionGradeAsync(request);
        return CreatedSuccess(grade, $"สร้างเกรดสภาพ '{grade.GradeCode}' สำเร็จ");
    }

    [HttpPut("condition-grades/{id:guid}")]
    public async Task<IActionResult> UpdateConditionGrade(Guid id, [FromBody] UpdateConditionGradeRequest request)
    {
        var grade = await _masterDataService.UpdateConditionGradeAsync(id, request);
        return Success(grade, $"แก้ไขเกรดสภาพ '{grade.GradeCode}' สำเร็จ");
    }

    [HttpDelete("condition-grades/{id:guid}")]
    public async Task<IActionResult> DeleteConditionGrade(Guid id)
    {
        await _masterDataService.DeleteConditionGradeAsync(id);
        return Success("ลบเกรดสภาพสำเร็จ");
    }

    // ==========================================
    // PRODUCTS
    // ==========================================

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] Guid? brandId,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search)
    {
        var products = await _masterDataService.GetProductsAsync(brandId, categoryId, search);
        return Success(products, "ดึงรายการสินค้ามาสเตอร์สำเร็จ");
    }

    [HttpGet("products/{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _masterDataService.GetProductByIdAsync(id);
        return Success(product, "ดึงข้อมูลรุ่นสินค้าสำเร็จ");
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = await _masterDataService.CreateProductAsync(request);
        return CreatedSuccess(product, $"สร้างรุ่นสินค้า '{product.ModelName}' สำเร็จ");
    }

    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _masterDataService.UpdateProductAsync(id, request);
        return Success(product, $"แก้ไขรุ่นสินค้า '{product.ModelName}' สำเร็จ");
    }

    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _masterDataService.DeleteProductAsync(id);
        return Success("ลบรุ่นสินค้าสำเร็จ");
    }
}
