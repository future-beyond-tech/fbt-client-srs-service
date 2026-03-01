using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRS.Application.DTOs;
using SRS.Application.Features.ManualBilling.CreateManualBill;
using SRS.Application.Features.ManualBilling.GetManualBillByNumber;
using SRS.Application.Features.ManualBilling.GetManualBillInvoice;
using SRS.Application.Features.ManualBilling.SendManualBillInvoice;
using SRS.Application.Interfaces;

namespace SRS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/manual-bills")]
public class ManualBillsController(
    ICreateManualBillHandler createHandler,
    IGetManualBillByNumberHandler getByNumberHandler,
    IGetManualBillInvoiceHandler getInvoiceHandler,
    IManualBillInvoicePdfService pdfService,
    ISendManualBillInvoiceHandler sendInvoiceHandler) : ControllerBase
{
    /// <summary>Create a manual bill. Payment split (cash + upi + finance) must equal amount total.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateManualBillResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] ManualBillCreateDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateManualBillCommand(dto);
        var result = await createHandler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetByBillNumber), new { billNumber = result.BillNumber }, result);
    }

    /// <summary>Get manual bill by bill number.</summary>
    [HttpGet("{billNumber:int}")]
    [ProducesResponseType(typeof(ManualBillDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByBillNumber(int billNumber, CancellationToken cancellationToken)
    {
        var query = new GetManualBillByNumberQuery(billNumber);
        var result = await getByNumberHandler.Handle(query, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Get invoice DTO for PDF generation.</summary>
    [HttpGet("{billNumber:int}/invoice")]
    [ProducesResponseType(typeof(ManualBillInvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInvoice(int billNumber, CancellationToken cancellationToken)
    {
        var query = new GetManualBillInvoiceQuery(billNumber);
        var result = await getInvoiceHandler.Handle(query, cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Get or create PDF URL for the manual bill. Idempotent: returns existing URL if already generated. Optional ?redirect=true to redirect to the PDF. When redirect=false, returns PDF file (application/pdf) or JSON with PdfUrl depending on Accept.</summary>
    [HttpGet("{billNumber:int}/pdf")]
    [ProducesResponseType(typeof(ManualBillPdfResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetPdf(int billNumber, [FromQuery] bool redirect = false, [FromQuery] bool download = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (redirect)
            {
                var pdfUrl = await pdfService.GetOrCreatePdfUrlAsync(billNumber, cancellationToken);
                return Redirect(pdfUrl);
            }

            if (download)
            {
                var pdfBytes = await pdfService.GetPdfBytesAsync(billNumber, cancellationToken);
                return File(pdfBytes, "application/pdf", $"manual-invoice-{billNumber}.pdf");
            }

            var url = await pdfService.GetOrCreatePdfUrlAsync(billNumber, cancellationToken);
            return Ok(new ManualBillPdfResponse { PdfUrl = url });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            // Return 502 so client can show a generic message. Do not log exception detail (may contain URLs).
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.3",
                Title = "PDF generation failed",
                Status = StatusCodes.Status502BadGateway,
                Detail = "Unable to generate or retrieve the PDF. Please try again.",
                Instance = HttpContext.Request.Path,
            });
        }
    }

    /// <summary>Generate PDF (if needed), store URL, and send invoice to customer via WhatsApp. Idempotent: reuses existing PDF on repeated sends.</summary>
    [HttpPost("{billNumber:int}/send-invoice")]
    [ProducesResponseType(typeof(SendInvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> SendInvoice(int billNumber, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sendInvoiceHandler.Handle(new SendManualBillInvoiceCommand(billNumber), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
    }
}

internal sealed class ManualBillPdfResponse
{
    public string PdfUrl { get; set; } = null!;
}
