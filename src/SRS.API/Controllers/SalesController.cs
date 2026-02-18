using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/sales")]
public class SalesController(ISaleService saleService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleCreateDto dto)
    {
        try
        {
            var createdSale = await saleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByBill), new { billNumber = createdSale.BillNumber }, createdSale);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{billNumber}")]
    public async Task<IActionResult> GetByBill(string billNumber)
    {
        var result = await saleService.GetByBillNumberAsync(billNumber);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{billNumber}/detail")]
    public async Task<IActionResult> GetBillDetail(string billNumber)
    {
        var result = await saleService.GetBillDetailAsync(billNumber);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
