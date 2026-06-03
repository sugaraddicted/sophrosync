using MediatR;
using Sophrosync.Schedule.Application.DTOs;
using Sophrosync.Schedule.Domain.Enums;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Application.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler(
    IAvailabilityTemplateRepository templateRepository,
    IAppointmentRepository appointmentRepository,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser)
    : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotDto>>
{
    public async Task<IReadOnlyList<AvailableSlotDto>> Handle(
        GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        if (!currentTenant.HasTenant)
            throw new UnauthorizedAccessException("Tenant context is required.");

        var therapistId = currentUser.Id;
        var dateUtc = request.Date.ToUniversalTime().Date;
        var dayOfWeek = dateUtc.DayOfWeek;

        // Find the active template for this therapist and day.
        var templates = await templateRepository.GetByTherapistAsync(therapistId, cancellationToken);
        var activeTemplate = templates.FirstOrDefault(t => t.DayOfWeek == dayOfWeek && t.IsActive);

        if (activeTemplate is null)
            return Array.Empty<AvailableSlotDto>();

        // Build candidate slots by splitting the template window.
        var slots = new List<AvailableSlotDto>();
        var windowStart = dateUtc.Add(activeTemplate.StartTime.ToTimeSpan());
        var windowEnd   = dateUtc.Add(activeTemplate.EndTime.ToTimeSpan());
        var slotDuration = TimeSpan.FromMinutes(request.SlotDurationMinutes);

        var slotStart = windowStart;
        while (slotStart + slotDuration <= windowEnd)
        {
            slots.Add(new AvailableSlotDto(slotStart, slotStart + slotDuration));
            slotStart += slotDuration;
        }

        if (slots.Count == 0)
            return Array.Empty<AvailableSlotDto>();

        // Load appointments for this day and filter to this therapist's non-cancelled ones.
        var dayAppointments = await appointmentRepository.GetByDateRangeAsync(
            dateUtc, dateUtc.AddDays(1), cancellationToken);

        var occupiedAppointments = dayAppointments
            .Where(a => a.TherapistId == therapistId
                        && a.Status != AppointmentStatus.Cancelled
                        && a.Status != AppointmentStatus.NoShow)
            .ToList();

        // Remove slots that overlap any occupied appointment.
        // Overlap condition: slot.Start < apptEnd && slot.End > appt.ScheduledAt
        var availableSlots = slots
            .Where(slot => !occupiedAppointments.Any(appt =>
            {
                var apptEnd = appt.ScheduledAt.AddMinutes(appt.DurationMinutes);
                return slot.Start < apptEnd && slot.End > appt.ScheduledAt;
            }))
            .ToList();

        return availableSlots.AsReadOnly();
    }
}
