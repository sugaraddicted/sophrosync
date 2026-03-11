using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Consent.Application.Commands.CreateConsentTemplate;
using Sophrosync.Consent.Application.Commands.PublishConsentTemplate;
using Sophrosync.Consent.Application.Commands.RetireConsentTemplate;
using Sophrosync.Consent.Application.Queries.GetConsentTemplate;
using Sophrosync.Consent.Application.Queries.ListConsentTemplates;

namespace Sophrosync.Consent.API.Controllers;

[ApiController]
[Route("api/consent-templates")]
[Authorize]
public sealed class ConsentTemplatesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct = default)
        => Ok(await mediator.Send(new ListConsentTemplatesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetConsentTemplateQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateConsentTemplateCommand command, CancellationToken ct = default)
    {
        var id = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct = default)
    {
        await mediator.Send(new PublishConsentTemplateCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/retire")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Retire(Guid id, CancellationToken ct = default)
    {
        await mediator.Send(new RetireConsentTemplateCommand(id), ct);
        return NoContent();
    }
}
