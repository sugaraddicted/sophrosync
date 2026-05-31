namespace Sophrosync.Schedule.Application.DTOs;

/// <summary>DTO returned for an availability template.</summary>
public sealed record AvailabilityTemplateDto(
    Guid Id,
    Guid TherapistId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);
