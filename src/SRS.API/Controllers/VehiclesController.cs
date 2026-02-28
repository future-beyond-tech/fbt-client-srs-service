using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.API.Controllers;


[ApiController]
[Route("api/vehicles")]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    private const int MaxPhotosPerRequest = 5;
    private const long MaxFileSize = 2 * 1024 * 1024;
    private const long MaxPhotoRequestSize = MaxPhotosPerRequest * MaxFileSize;

    public sealed class UploadVehiclePhotosRequest
    {
        public List<IFormFile> Files { get; set; } = [];
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vehicles = await vehicleService.GetAllAsync();
        return Ok(vehicles);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vehicle = await vehicleService.GetByIdAsync(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        return Ok(vehicle);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var vehicles = await vehicleService.GetAvailableAsync();
        return Ok(vehicles);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{vehicleId:int}/photos")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxPhotoRequestSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxPhotoRequestSize)]
    public async Task<IActionResult> UploadPhotos(int vehicleId, [FromForm] UploadVehiclePhotosRequest request)
    {
        try
        {
            var files = request?.Files ?? [];
            var uploadedPhotos = await vehicleService.UploadPhotosAsync(vehicleId, files);
            return Ok(uploadedPhotos);
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{vehicleId:int}/photos/{photoId:int}/primary")]
    public async Task<IActionResult> SetPrimaryPhoto(int vehicleId, int photoId)
    {
        try
        {
            var updatedPhotos = await vehicleService.SetPrimaryPhotoAsync(vehicleId, photoId);
            return Ok(new { vehicleId, photos = updatedPhotos });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("photos/{photoId:int}")]
    public async Task<IActionResult> DeletePhoto(int photoId)
    {
        try
        {
            var updatedPhotos = await vehicleService.DeletePhotoAsync(photoId);
            return Ok(new { photos = updatedPhotos });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] VehicleUpdateDto dto)
    {
        try
        {
            var updatedVehicle = await vehicleService.UpdateVehicleAsync(id, dto);
            return Ok(updatedVehicle);
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
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        try
        {
            await vehicleService.SoftDeleteVehicleAsync(id);
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
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] VehicleStatusUpdateDto dto)
    {
        try
        {
            var updatedVehicle = await vehicleService.UpdateVehicleStatusAsync(id, dto);
            return Ok(updatedVehicle);
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
