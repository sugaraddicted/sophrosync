using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sophrosync.Notes.API.Controllers;

/// <summary>
/// Internal endpoint consumed by ReportingService.
/// Stub — returns placeholder data until NotesService is fully implemented.
/// Blocked from public access by YARP gateway.
/// </summary>
[ApiController]
[Route("internal/notes")]
[Authorize]
public sealed class InternalNotesController : ControllerBase
{
    [HttpGet("summary")]
    public IActionResult GetSummary(
        [FromQuery] Guid tenantId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        // TODO: implement when NotesService domain + application layers are scaffolded
        return Ok(new
        {
            TotalAppointments = 0,
            NotesCreated = 0,
            NotesSigned = 0,
            NotesOverdue = 0,
            CompletionRate = 0.0,
            PeriodStart = from,
            PeriodEnd = to
        });
    }
}
