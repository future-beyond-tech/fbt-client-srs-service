using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRS.Application.Interfaces;

namespace SRS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/upload")]
public class UploadController(ICustomerPhotoStorageService fileStorageService) : ControllerBase
{
    private const long MaxFileSize = 2 * 1024 * 1024;

    public sealed class UploadFileRequest
    {
        public IFormFile File { get; set; } = null!;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
    public async Task<IActionResult> Upload([FromForm] UploadFileRequest request)
    {
        try
        {
            var url = await fileStorageService.SaveCustomerPhotoAsync(request.File);
            return Ok(new { url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
