using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Notifications.Application.Commands.SetTenantDefaults;
using Sophrosync.Notifications.Application.Commands.UpdatePreference;
using Sophrosync.Notifications.Application.Queries.GetPreference;
using Sophrosync.Notifications.Application.DTOs;
using Sophrosync.Notifications.Domain.Enums;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Notifications.API.Controllers;

[ApiController]
[Route("api/notification-preferences")]
[Authorize]
public sealed class NotificationPreferencesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyPreference(CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        var userId = User.GetUserId();
        var result = await mediator.Send(new GetNotificationPreferenceQuery(tenantId, userId), ct)
            ?? new NotificationPreferenceDto(Guid.Empty, tenantId, userId, NotificationChannel.InApp,
                EmailEnabled: false, InAppEnabled: true, SmsEnabled: false, EmailAddress: null);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePreference(
        [FromBody] UpdateNotificationPreferenceCommand command, CancellationToken ct = default)
    {
        await mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut("tenant-defaults")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> SetTenantDefaults(
        [FromBody] SetTenantNotificationDefaultsCommand command, CancellationToken ct = default)
    {
        await mediator.Send(command, ct);
        return NoContent();
    }
}
