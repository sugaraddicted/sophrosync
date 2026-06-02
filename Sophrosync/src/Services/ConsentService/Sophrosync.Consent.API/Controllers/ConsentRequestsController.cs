using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Consent.Application.Commands.IssueConsentRequest;
using Sophrosync.Consent.Application.Commands.RecordConsentGranted;
using Sophrosync.Consent.Application.Commands.RevokeConsentRequest;
using Sophrosync.Consent.Application.Commands.WithdrawConsent;
using Sophrosync.Consent.Application.Queries.ListClientConsentHistory;
using Sophrosync.Consent.Application.Queries.ListPendingConsentRequests;

namespace Sophrosync.Consent.API.Controllers;

[ApiController]
[Route("api/consent-requests")]
[Authorize]
public sealed class ConsentRequestsController(IMediator mediator) : ControllerBase
{
    [HttpGet("client/{clientId:guid}/pending")]
    public async Task<IActionResult> GetPending(Guid clientId, CancellationToken ct = default)
        => Ok(await mediator.Send(new ListPendingConsentRequestsQuery(clientId), ct));

    [HttpGet("client/{clientId:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid clientId, CancellationToken ct = default)
        => Ok(await mediator.Send(new ListClientConsentHistoryQuery(clientId), ct));

    [HttpPost]
    [Authorize(Roles = "admin,therapist")]
    public async Task<IActionResult> Issue([FromBody] IssueConsentRequestCommand command, CancellationToken ct = default)
    {
        var id = await mediator.Send(command, ct);
        return Ok(new { Id = id });
    }

    [HttpPost("{id:guid}/grant")]
    public async Task<IActionResult> Grant(Guid id, CancellationToken ct = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var recordId = await mediator.Send(new RecordConsentGrantedCommand(id, ipAddress), ct);
        return Ok(new { RecordId = recordId });
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, [FromQuery] Guid clientId, CancellationToken ct = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var recordId = await mediator.Send(new WithdrawConsentCommand(clientId, id, ipAddress), ct);
        return Ok(new { RecordId = recordId });
    }

    [HttpPost("{id:guid}/revoke")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct = default)
    {
        await mediator.Send(new RevokeConsentRequestCommand(id), ct);
        return NoContent();
    }
}
