namespace Sophrosync.Reporting.Application.DTOs;

public sealed record NoteCompletionRateDto(
    int TotalAppointments,
    int NotesCreated,
    int NotesSigned,
    int NotesOverdue,
    double CompletionRate,
    DateTime PeriodStart,
    DateTime PeriodEnd);
