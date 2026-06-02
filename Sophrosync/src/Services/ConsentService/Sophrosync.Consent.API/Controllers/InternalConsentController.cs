using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Consent.Application.Queries.GetConsentAuditSummary;
using Sophrosync.Consent.Application.Queries.GetConsentStatus;
using Sophrosync.Consent.Domain.Enums;

namespace Sophrosync.Consent.API.Controllers;

/// <summary>
/// Internal endpoint — called by other services to check consent status.
/// Blocked from public access by YARP gateway.
/// </summary>
[ApiController]
[Route("internal/consent")]
[Authorize]
public sealed class InternalConsentController(IMediator mediator) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(
        [FromQuery] Guid tenantId,
        [FromQuery] Guid clientId,
        [FromQuery] ConsentPurpose purpose,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetConsentStatusQuery(tenantId, clientId, purpose), ct);
        return Ok(result);
    }

    [HttpGet("audit-summary")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAuditSummary(CancellationToken ct = default)
        => Ok(await mediator.Send(new GetConsentAuditSummaryQuery(), ct));
}
