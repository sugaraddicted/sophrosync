using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Clients.Application.Commands.CreateClient;
using Sophrosync.Clients.Application.Commands.DeleteClient;
using Sophrosync.Clients.Application.Commands.UpdateClient;
using Sophrosync.Clients.Application.Queries.GetClientById;
using Sophrosync.Clients.Application.Queries.GetClients;

namespace Sophrosync.Clients.API.Controllers;

/// <summary>
/// Public CRUD endpoints for the Client resource.
/// Authorization is intentionally omitted for local development / SPA integration.
/// Re-enable [Authorize] once Keycloak is wired up.
/// </summary>
[ApiController]
[Route("api/clients")]
public sealed class ClientsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await mediator.Send(new GetClientsQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetClientByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand command, CancellationToken ct = default)
    {
        var dto = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest body, CancellationToken ct = default)
    {
        var command = new UpdateClientCommand(id, body.Name, body.Email, body.Phone, body.Status);
        var result = await mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var deleted = await mediator.Send(new DeleteClientCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

/// <summary>
/// Request body for PUT /api/clients/{id}.
/// Separates the route-bound Id from the body payload.
/// </summary>
public sealed record UpdateClientRequest(
    string Name,
    string Email,
    string Phone,
    string Status);
