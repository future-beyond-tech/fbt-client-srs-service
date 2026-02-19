using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/purchases")]
public class PurchasesController(IPurchaseService purchaseService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseCreateDto dto)
    {
        try
        {
            var createdPurchase = await purchaseService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdPurchase.Id }, createdPurchase);
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var purchases = await purchaseService.GetAllAsync();
        return Ok(purchases);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var purchase = await purchaseService.GetByIdAsync(id);
        if (purchase is null)
        {
            return NotFound();
        }

        return Ok(purchase);
    }
}
