using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/sales")]
public class SalesController(ISaleService saleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var result = await saleService.GetHistoryAsync(pageNumber, pageSize, search, fromDate, toDate);
        return Ok(result);
    }

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
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ApplicationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{billNumber}")]
    public async Task<IActionResult> GetByBill(int billNumber)
    {
        var result = await saleService.GetByBillNumberAsync(billNumber);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{billNumber}/invoice")]
    public async Task<IActionResult> GetInvoice(int billNumber)
    {
        var result = await saleService.GetInvoiceAsync(billNumber);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("{billNumber}/send-invoice")]
    public async Task<IActionResult> SendInvoice(int billNumber, CancellationToken cancellationToken)
    {
        try
        {
            var result = await saleService.SendInvoiceAsync(billNumber, cancellationToken);
            return Ok(result);
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
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }

    [HttpPost("{billNumber}/process-invoice")]
    public async Task<IActionResult> ProcessInvoice(int billNumber, CancellationToken cancellationToken)
    {
        try
        {
            var result = await saleService.ProcessInvoiceAsync(billNumber, cancellationToken);
            return Ok(result);
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
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }
}
