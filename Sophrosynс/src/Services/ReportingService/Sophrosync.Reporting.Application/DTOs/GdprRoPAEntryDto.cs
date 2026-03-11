namespace Sophrosync.Reporting.Application.DTOs;

public sealed record GdprRoPAEntryDto(
    string ProcessingActivity,
    string Purpose,
    string LegalBasis,
    string DataCategories,
    string DataSubjects,
    string RetentionPeriod,
    string SecurityMeasures);
