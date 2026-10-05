using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Models;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[Authorize]
[Route("api/customers")]
public class CustomersController : ApiControllerBase
{
    private readonly IMasterDataService _masterDataService;

    public CustomersController(IMasterDataService masterDataService)
    {
        _masterDataService = masterDataService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] string? search)
    {
        var customers = await _masterDataService.GetCustomersAsync(search);
        return Success(customers, "ดึงรายชื่อลูกค้าสำเร็จ");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        var customer = await _masterDataService.GetCustomerByIdAsync(id);
        return Success(customer, "ดึงข้อมูลลูกค้าสำเร็จ");
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var customer = await _masterDataService.CreateCustomerAsync(request);
        return CreatedSuccess(customer, $"บันทึกข้อมูลลูกค้า '{customer.Name}' สำเร็จ");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var customer = await _masterDataService.UpdateCustomerAsync(id, request);
        return Success(customer, $"แก้ไขข้อมูลลูกค้า '{customer.Name}' สำเร็จ");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await _masterDataService.DeleteCustomerAsync(id);
        return Success("ลบข้อมูลลูกค้าสำเร็จ");
    }
}
