using MediatR;
using Sophrosync.Schedule.Application.DTOs;

namespace Sophrosync.Schedule.Application.Queries.GetAvailableSlots;

/// <summary>
/// Returns available booking slots for the requesting therapist on a given date.
/// Slots are computed by splitting the active availability template window
/// and removing any that overlap existing non-cancelled appointments.
/// </summary>
public sealed record GetAvailableSlotsQuery(
    DateTime Date,
    int SlotDurationMinutes) : IRequest<IReadOnlyList<AvailableSlotDto>>;
