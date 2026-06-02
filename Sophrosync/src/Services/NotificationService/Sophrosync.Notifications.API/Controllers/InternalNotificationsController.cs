using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Notifications.Application.Commands.SendNotification;

namespace Sophrosync.Notifications.API.Controllers;

/// <summary>
/// Internal endpoint — blocked from public access by YARP gateway.
/// Used by ScheduleService, ClientService, NotesService, ConsentService.
/// </summary>
[ApiController]
[Route("internal/notifications")]
[Authorize]
public sealed class InternalNotificationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendNotificationCommand command, CancellationToken ct = default)
    {
        var notificationId = await mediator.Send(command, ct);
        return Ok(new { NotificationId = notificationId });
    }
}
