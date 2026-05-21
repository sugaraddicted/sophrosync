using Sophrosync.Schedule.Domain.Enums;
using Sophrosync.Schedule.Domain.Events;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Schedule.Domain.Entities;

public sealed class Appointment : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid TherapistId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public int DurationMinutes { get; private set; }
    public AppointmentType Type { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }

    private Appointment() { }

    public static Appointment Schedule(
        Guid tenantId,
        Guid clientId,
        Guid therapistId,
        DateTime scheduledAt,
        int durationMinutes,
        AppointmentType type,
        string? notes = null)
    {
        if (durationMinutes <= 0)
            throw new ArgumentException("Duration must be positive.", nameof(durationMinutes));

        var appointment = new Appointment
        {
            TenantId = tenantId,
            ClientId = clientId,
            TherapistId = therapistId,
            ScheduledAt = scheduledAt,
            DurationMinutes = durationMinutes,
            Type = type,
            Status = AppointmentStatus.Scheduled,
            Notes = notes,
        };

        appointment.RaiseDomainEvent(new AppointmentScheduledDomainEvent(
            appointment.Id, tenantId, clientId, therapistId, scheduledAt));

        return appointment;
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled appointments can be confirmed.");
        Status = AppointmentStatus.Confirmed;
    }

    public void Complete(string? notes = null)
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
            throw new InvalidOperationException("Only scheduled or confirmed appointments can be completed.");
        Status = AppointmentStatus.Completed;
        if (notes is not null) Notes = notes;
    }

    public void Cancel(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a completed or already cancelled appointment.");
        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        RaiseDomainEvent(new AppointmentCancelledDomainEvent(Id, TenantId, reason));
    }

    public void MarkNoShow()
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
            throw new InvalidOperationException("Only scheduled or confirmed appointments can be marked as no-show.");
        Status = AppointmentStatus.NoShow;
    }

    public void Reschedule(DateTime newScheduledAt, int? newDurationMinutes = null)
    {
        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot reschedule a completed or cancelled appointment.");
        if (newDurationMinutes.HasValue && newDurationMinutes.Value <= 0)
            throw new ArgumentException("Duration must be positive.", nameof(newDurationMinutes));

        ScheduledAt = newScheduledAt;
        if (newDurationMinutes.HasValue) DurationMinutes = newDurationMinutes.Value;
        Status = AppointmentStatus.Scheduled;
    }
}
