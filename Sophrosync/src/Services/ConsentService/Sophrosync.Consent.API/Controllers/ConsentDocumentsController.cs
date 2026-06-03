using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Consent.Application.Commands.AttachConsentDocument;
using Sophrosync.Consent.Application.Queries.GetConsentDocument;

namespace Sophrosync.Consent.API.Controllers;

[ApiController]
[Route("api/consent-records")]
[Authorize]
public sealed class ConsentDocumentsController(IMediator mediator) : ControllerBase
{
    [HttpPost("{recordId:guid}/document")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Attach(Guid recordId, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        var id = await mediator.Send(
            new AttachConsentDocumentCommand(recordId, file.FileName, file.ContentType, ms.ToArray()), ct);

        return Ok(new { Id = id });
    }

    [HttpGet("{recordId:guid}/document")]
    public async Task<IActionResult> Download(Guid recordId, CancellationToken ct = default)
    {
        var doc = await mediator.Send(new GetConsentDocumentQuery(recordId), ct);
        if (doc is null) return NotFound();
        return File(doc.Data, doc.ContentType, doc.FileName);
    }
}
