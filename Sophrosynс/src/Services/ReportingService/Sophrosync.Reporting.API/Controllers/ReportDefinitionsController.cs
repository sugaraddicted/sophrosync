using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Reporting.Application.Commands.CreateReportDefinition;
using Sophrosync.Reporting.Application.Commands.DeleteReportRun;
using Sophrosync.Reporting.Application.Commands.TriggerReportRun;
using Sophrosync.Reporting.Application.Queries.GetReportRun;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Reporting.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportDefinitionsController(IMediator mediator) : ControllerBase
{
    [HttpPost("definitions")]
    [Authorize(Roles = "admin,supervisor")]
    public async Task<IActionResult> Create(
        [FromBody] CreateReportDefinitionCommand command, CancellationToken ct = default)
    {
        var id = await mediator.Send(command, ct);
        return Ok(new { Id = id });
    }

    [HttpPost("{definitionId:guid}/run")]
    [Authorize(Roles = "admin,supervisor,therapist")]
    public async Task<IActionResult> TriggerRun(
        Guid definitionId,
        [FromBody] TriggerRunRequest request,
        CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        var userId = User.GetUserId();
        var runId = await mediator.Send(
            new TriggerReportRunCommand(tenantId, definitionId, userId, request.PeriodStart, request.PeriodEnd), ct);
        return Accepted(new { RunId = runId });
    }

    [HttpGet("runs/{runId:guid}")]
    public async Task<IActionResult> GetRun(Guid runId, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetReportRunQuery(runId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("runs/{runId:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteRun(Guid runId, CancellationToken ct = default)
    {
        await mediator.Send(new DeleteReportRunCommand(runId), ct);
        return NoContent();
    }

    public sealed record TriggerRunRequest(DateTime PeriodStart, DateTime PeriodEnd);
}
