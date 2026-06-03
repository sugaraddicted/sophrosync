using Sophrosync.Schedule.Domain.Entities;

namespace Sophrosync.Schedule.Application.DTOs;

public sealed class AppointmentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid ClientId { get; init; }
    public Guid TherapistId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime CreatedAt { get; init; }

    public static AppointmentDto FromAppointment(Appointment a) => new()
    {
        Id = a.Id,
        TenantId = a.TenantId,
        ClientId = a.ClientId,
        TherapistId = a.TherapistId,
        ScheduledAt = a.ScheduledAt,
        DurationMinutes = a.DurationMinutes,
        Type = a.Type.ToString(),
        Status = a.Status.ToString(),
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt,
    };
}
