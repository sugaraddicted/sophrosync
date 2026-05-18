using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Clients.API.Controllers;

[ApiController]
[Route("internal/clients")]
[Authorize(Policy = "CanReadClients")]
public sealed class InternalClientsController(ICurrentTenant currentTenant) : ControllerBase
{
    [HttpGet("summary")]
    public IActionResult GetSummary([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        // TenantId is resolved from the JWT claim — callers cannot spoof it via query string.
        // TODO: replace stub with real queries once ClientService application layer is complete.
        return Ok(new
        {
            TenantId = currentTenant.Id,
            PeriodStart = from,
            PeriodEnd = to,
            TotalClientsActive = 0,
            TotalSessionsCompleted = 0,
            TotalTreatmentPlansActive = 0,
            GoalsAchieved = 0,
            GoalsInProgress = 0,
            AverageSessionsPerClient = 0.0
        });
    }
}
