using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sophrosync.Schedule.API.Controllers;

/// <summary>
/// Internal endpoint consumed by ReportingService.
/// Stub — returns placeholder data until ScheduleService is fully implemented.
/// Blocked from public access by YARP gateway.
/// </summary>
[ApiController]
[Route("internal/appointments")]
[Authorize]
public sealed class InternalAppointmentsController : ControllerBase
{
    [HttpGet("summary")]
    public IActionResult GetSummary(
        [FromQuery] Guid tenantId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        // TODO: implement when ScheduleService domain + application layers are scaffolded
        return Ok(new
        {
            TotalScheduled = 0,
            TotalCompleted = 0,
            TotalCancelled = 0,
            TotalNoShow = 0,
            PeriodStart = from,
            PeriodEnd = to
        });
    }
}
