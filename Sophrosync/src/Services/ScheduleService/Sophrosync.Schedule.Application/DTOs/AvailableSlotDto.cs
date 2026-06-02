namespace Sophrosync.Schedule.Application.DTOs;

/// <summary>Represents a single bookable time slot.</summary>
public sealed record AvailableSlotDto(DateTime Start, DateTime End);
