using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Notes.Application.Commands.AmendNote;
using Sophrosync.Notes.Application.Commands.CreateNote;
using Sophrosync.Notes.Application.Commands.DeleteNote;
using Sophrosync.Notes.Application.Commands.LockNote;
using Sophrosync.Notes.Application.Commands.RequestCoSign;
using Sophrosync.Notes.Application.Commands.SignNote;
using Sophrosync.Notes.Application.Commands.UpdateNote;
using Sophrosync.Notes.Application.Queries.GetNoteById;
using Sophrosync.Notes.Application.Queries.GetNotes;
using Sophrosync.Notes.Application.Queries.GetNotesByClientId;

namespace Sophrosync.Notes.API.Controllers;

/// <summary>
/// Public CRUD and lifecycle endpoints for the Note resource.
/// </summary>
[ApiController]
[Route("api/notes")]
[Authorize(Policy = "CanReadNotes")]
public sealed class NotesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await mediator.Send(new GetNotesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetNoteByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("client/{clientId:guid}")]
    public async Task<IActionResult> GetByClientId(Guid clientId, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetNotesByClientIdQuery(clientId), ct));

    [HttpPost]
    [Authorize(Policy = "CanManageNotes")]
    public async Task<IActionResult> Create([FromBody] CreateNoteCommand command, CancellationToken ct = default)
    {
        var dto = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageNotes")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteRequest body, CancellationToken ct = default)
    {
        var command = new UpdateNoteCommand(id, body.Title, body.Content, body.Tags);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/sign")]
    [Authorize(Policy = "CanSignNotes")]
    public async Task<IActionResult> Sign(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new SignNoteCommand(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/lock")]
    [Authorize(Policy = "CanSignNotes")]
    public async Task<IActionResult> Lock(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new LockNoteCommand(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/request-cosign")]
    [Authorize(Policy = "CanSignNotes")]
    public async Task<IActionResult> RequestCoSign(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new RequestCoSignCommand(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/amend")]
    [Authorize(Policy = "CanManageNotes")]
    public async Task<IActionResult> Amend(Guid id, [FromBody] AmendNoteRequest body, CancellationToken ct = default)
    {
        var command = new AmendNoteCommand(id, body.Title, body.Content, body.Tags);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanManageNotes")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var deleted = await mediator.Send(new DeleteNoteCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

/// <summary>
/// Request body for PUT /api/notes/{id}.
/// Separates the route-bound Id from the body payload.
/// </summary>
public sealed record UpdateNoteRequest(string? Title, string Content, string? Tags);

/// <summary>
/// Request body for POST /api/notes/{id}/amend.
/// Carries the content for the new Draft note that replaces the Locked original.
/// </summary>
public sealed record AmendNoteRequest(string? Title, string Content, string? Tags);
