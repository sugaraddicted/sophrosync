using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Reporting.Domain.ValueObjects;

public sealed class ReportSchedule : ValueObject
{
    public bool IsScheduled { get; init; }
    public DayOfWeek? DayOfWeek { get; init; }
    public TimeOnly? TimeOfDay { get; init; }

    private ReportSchedule() { }

    public static ReportSchedule None() => new() { };

    public static ReportSchedule Weekly(DayOfWeek day, TimeOnly time) =>
        new() { IsScheduled = true, DayOfWeek = day, TimeOfDay = time };

    public static ReportSchedule Daily(TimeOnly time) =>
        new() { IsScheduled = true, TimeOfDay = time };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IsScheduled;
        yield return DayOfWeek;
        yield return TimeOfDay;
    }
}
