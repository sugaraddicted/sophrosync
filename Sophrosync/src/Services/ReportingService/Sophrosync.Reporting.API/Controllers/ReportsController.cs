using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Reporting.Application.Queries.GetAppointmentSummary;
using Sophrosync.Reporting.Application.Queries.GetClinicalOutcomeSummary;
using Sophrosync.Reporting.Application.Queries.GetGdprRoPA;
using Sophrosync.Reporting.Application.Queries.GetNoteCompletionRate;
using Sophrosync.Reporting.Application.Queries.GetPracticeAnalytics;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Reporting.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("clinical-outcomes")]
    [Authorize(Roles = "admin,supervisor,therapist")]
    public async Task<IActionResult> GetClinicalOutcomes(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        return Ok(await mediator.Send(new GetClinicalOutcomeSummaryQuery(tenantId, from, to), ct));
    }

    [HttpGet("practice-analytics")]
    [Authorize(Roles = "admin,supervisor")]
    public async Task<IActionResult> GetPracticeAnalytics(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        return Ok(await mediator.Send(new GetPracticeAnalyticsQuery(tenantId, from, to), ct));
    }

    [HttpGet("gdpr-ropa")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetGdprRoPA(CancellationToken ct = default)
        => Ok(await mediator.Send(new GetGdprRoPAQuery(), ct));

    [HttpGet("appointment-summary")]
    [Authorize(Roles = "admin,supervisor,therapist")]
    public async Task<IActionResult> GetAppointmentSummary(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        return Ok(await mediator.Send(new GetAppointmentSummaryQuery(tenantId, from, to), ct));
    }

    [HttpGet("note-completion-rate")]
    [Authorize(Roles = "admin,supervisor,therapist")]
    public async Task<IActionResult> GetNoteCompletionRate(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        return Ok(await mediator.Send(new GetNoteCompletionRateQuery(tenantId, from, to), ct));
    }
}
