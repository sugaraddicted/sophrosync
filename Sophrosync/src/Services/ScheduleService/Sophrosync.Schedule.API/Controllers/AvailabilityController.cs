using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Schedule.Application.Commands.DeactivateAvailability;
using Sophrosync.Schedule.Application.Commands.SetAvailability;
using Sophrosync.Schedule.Application.DTOs;
using Sophrosync.Schedule.Application.Queries.GetAvailabilityTemplates;
using Sophrosync.Schedule.Application.Queries.GetAvailableSlots;

namespace Sophrosync.Schedule.API.Controllers;

[ApiController]
[Route("api/availability")]
[Authorize(Policy = "CanManageAppointments")]
public sealed class AvailabilityController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns all availability templates for the requesting therapist.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AvailabilityTemplateDto>>> GetTemplates(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAvailabilityTemplatesQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Sets availability for a given day of week. Deactivates any existing active template for that day
    /// and creates a new one. Returns the new template id.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SetAvailability([FromBody] SetAvailabilityCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetTemplates), new { }, new { id });
    }

    /// <summary>Deactivates a specific availability template.</summary>
    [HttpDelete("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeactivateAvailabilityCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Returns available booking slots for the requesting therapist on a given date,
    /// split into chunks of the specified duration and with occupied appointments removed.
    /// </summary>
    [HttpGet("slots")]
    public async Task<ActionResult<IReadOnlyList<AvailableSlotDto>>> GetSlots(
        [FromQuery] DateTime date,
        [FromQuery] int slotDurationMinutes,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetAvailableSlotsQuery(date, slotDurationMinutes), ct);
        return Ok(result);
    }
}
