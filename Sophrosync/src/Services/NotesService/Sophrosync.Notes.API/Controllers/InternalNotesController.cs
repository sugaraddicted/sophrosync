using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Domain.Interfaces;

namespace Sophrosync.Notes.API.Controllers;

/// <summary>
/// Internal endpoint consumed by ReportingService.
/// Blocked from public access by YARP gateway.
/// </summary>
[ApiController]
[Route("internal/notes")]
[Authorize]
public sealed class InternalNotesController(INoteRepository noteRepository) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct = default)
    {
        // Single GROUP BY query across all tenants — bypasses the global tenant filter intentionally.
        var counts = await noteRepository.GetStatusCountsGlobalAsync(ct);

        var draftNotes    = counts.GetValueOrDefault(NoteStatus.Draft);
        var pendingCoSign = counts.GetValueOrDefault(NoteStatus.PendingCoSign);
        var signedNotes   = counts.GetValueOrDefault(NoteStatus.Signed);
        var lockedNotes   = counts.GetValueOrDefault(NoteStatus.Locked);

        // Amended notes are excluded from the denominator: they represent superseded records,
        // not active clinical work. Including them would deflate the completion rate unfairly.
        var totalNotes = counts
            .Where(kv => kv.Key != NoteStatus.Amended)
            .Sum(kv => kv.Value);

        var completionRate = totalNotes > 0
            ? (double)lockedNotes / totalNotes
            : 0.0;

        return Ok(new NoteCompletionRateSummaryDto(
            totalNotes,
            lockedNotes,
            signedNotes,
            draftNotes,
            pendingCoSign,
            completionRate));
    }
}

/// <summary>
/// Summary DTO returned by the internal notes summary endpoint.
/// CompletionRate = LockedNotes / TotalNotes (0 when no notes exist).
/// </summary>
public sealed record NoteCompletionRateSummaryDto(
    int TotalNotes,
    int LockedNotes,
    int SignedNotes,
    int DraftNotes,
    int PendingCoSign,
    double CompletionRate);
