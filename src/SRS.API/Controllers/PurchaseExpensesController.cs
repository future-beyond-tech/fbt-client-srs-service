using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/purchases")]
public class PurchaseExpensesController(
    IPurchaseExpenseService purchaseExpenseService) : ControllerBase
{
    [HttpPost("{vehicleId:int}/expenses")]
    public async Task<IActionResult> Create(int vehicleId, [FromBody] PurchaseExpenseCreateDto dto)
    {
        try
        {
            var created = await purchaseExpenseService.CreateAsync(vehicleId, dto);
            return StatusCode(StatusCodes.Status201Created, created);
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

    [HttpGet("{vehicleId:int}/expenses")]
    public async Task<IActionResult> GetByVehicle(int vehicleId)
    {
        try
        {
            var expenses = await purchaseExpenseService.GetByVehicleIdAsync(vehicleId);
            return Ok(expenses);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("expenses/{expenseId:int}")]
    public async Task<IActionResult> Delete(int expenseId)
    {
        try
        {
            await purchaseExpenseService.DeleteAsync(expenseId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
