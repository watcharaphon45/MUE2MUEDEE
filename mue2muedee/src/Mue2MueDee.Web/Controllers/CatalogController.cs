using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Models;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[AllowAnonymous]
[Route("api/catalog")]
public class CatalogController : ApiControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("{storeCode}")]
    [ProducesResponseType(typeof(ApiResponse<PublicStoreInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStoreInfo([FromRoute] string storeCode, CancellationToken ct)
    {
        var store = await _catalogService.GetStoreInfoByCodeAsync(storeCode, ct);
        return Success(store, "ดึงข้อมูลร้านค้าสำเร็จ");
    }

    [HttpGet("{storeCode}/items")]
    [ProducesResponseType(typeof(ApiResponse<PublicCatalogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCatalogItems(
        [FromRoute] string storeCode,
        [FromQuery] CatalogQueryParameters parameters,
        CancellationToken ct)
    {
        var catalog = await _catalogService.GetCatalogByStoreCodeAsync(storeCode, parameters, ct);
        return Success(catalog, "ดึงรายการสินค้าสำเร็จ");
    }

    [HttpGet("{storeCode}/items/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PublicCatalogItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCatalogItemDetail(
        [FromRoute] string storeCode,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var item = await _catalogService.GetCatalogItemDetailAsync(storeCode, id, ct);
        return Success(item, "ดึงข้อมูลสินค้าสำเร็จ");
    }
}
