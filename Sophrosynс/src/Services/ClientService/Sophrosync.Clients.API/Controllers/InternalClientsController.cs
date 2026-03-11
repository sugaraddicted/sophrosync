using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sophrosync.Clients.API.Controllers;

/// <summary>
/// Internal endpoint consumed by ReportingService.
/// Stub — returns placeholder data until ClientService is fully implemented.
/// Blocked from public access by YARP gateway.
/// </summary>
[ApiController]
[Route("internal/clients")]
[Authorize]
public sealed class InternalClientsController : ControllerBase
{
    [HttpGet("summary")]
    public IActionResult GetSummary(
        [FromQuery] Guid tenantId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        // TODO: implement when ClientService domain + application layers are scaffolded
        return Ok(new
        {
            TenantId = tenantId,
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
