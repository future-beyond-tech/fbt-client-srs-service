using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/settings/delivery-note")]
public class DeliveryNoteSettingsController(
    IDeliveryNoteSettingsService deliveryNoteSettingsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await deliveryNoteSettingsService.GetAsync();
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateDeliveryNoteSettingsDto dto)
    {
        var updatedSettings = await deliveryNoteSettingsService.UpdateAsync(dto);
        return Ok(updatedSettings);
    }
}
