using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Notifications.Application.Commands.DismissNotification;
using Sophrosync.Notifications.Application.Queries.GetById;
using Sophrosync.Notifications.Application.Queries.GetInbox;
using Sophrosync.Notifications.Application.Queries.GetUnreadCount;
using Sophrosync.Notifications.Application.Queries.ListSent;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Notifications.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var result = await mediator.Send(new GetInboxQuery(userId, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var count = await mediator.Send(new GetUnreadCountQuery(userId), ct);
        return Ok(new { Count = count });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetNotificationByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("sent")]
    [Authorize(Roles = "admin,supervisor")]
    public async Task<IActionResult> ListSent([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListSentNotificationsQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken ct = default)
    {
        await mediator.Send(new DismissNotificationCommand(id), ct);
        return NoContent();
    }
}
