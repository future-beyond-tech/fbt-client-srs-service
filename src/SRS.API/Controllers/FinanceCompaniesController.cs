using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/finance-companies")]
public class FinanceCompaniesController(IFinanceCompanyService financeCompanyService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FinanceCompanyCreateDto dto)
    {
        try
        {
            var created = await financeCompanyService.CreateAsync(dto);
            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companies = await financeCompanyService.GetAllAsync();
        return Ok(companies);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await financeCompanyService.SoftDeleteAsync(id);
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
