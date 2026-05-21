using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Schedule.Application.Commands.CancelAppointment;
using Sophrosync.Schedule.Application.Commands.CompleteAppointment;
using Sophrosync.Schedule.Application.Commands.ConfirmAppointment;
using Sophrosync.Schedule.Application.Commands.MarkNoShow;
using Sophrosync.Schedule.Application.Commands.RescheduleAppointment;
using Sophrosync.Schedule.Application.Commands.ScheduleAppointment;
using Sophrosync.Schedule.Application.DTOs;
using Sophrosync.Schedule.Application.Queries.GetAppointmentById;
using Sophrosync.Schedule.Application.Queries.GetAppointments;
using Sophrosync.Schedule.Application.Queries.GetAppointmentsByClientId;
using Sophrosync.Schedule.Application.Queries.GetAppointmentsByDateRange;

namespace Sophrosync.Schedule.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize(Policy = "CanReadAppointments")]
public sealed class AppointmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAppointmentsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAppointmentByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("client/{clientId:guid}")]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetByClientId(Guid clientId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAppointmentsByClientIdQuery(clientId), ct);
        return Ok(result);
    }

    [HttpGet("range")]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetByDateRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetAppointmentsByDateRangeQuery(from, to), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<ActionResult<AppointmentDto>> Schedule([FromBody] ScheduleAppointmentCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ConfirmAppointmentCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest body, CancellationToken ct)
    {
        await mediator.Send(new CancelAppointmentCommand(id, body.Reason), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteRequest body, CancellationToken ct)
    {
        await mediator.Send(new CompleteAppointmentCommand(id, body.Notes), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reschedule")]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleRequest body, CancellationToken ct)
    {
        await mediator.Send(new RescheduleAppointmentCommand(id, body.NewScheduledAt, body.NewDurationMinutes), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/no-show")]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> MarkNoShow(Guid id, CancellationToken ct)
    {
        await mediator.Send(new MarkNoShowCommand(id), ct);
        return NoContent();
    }
}

public sealed record CancelRequest(string Reason);
public sealed record CompleteRequest(string? Notes);
public sealed record RescheduleRequest(DateTime NewScheduledAt, int? NewDurationMinutes);
