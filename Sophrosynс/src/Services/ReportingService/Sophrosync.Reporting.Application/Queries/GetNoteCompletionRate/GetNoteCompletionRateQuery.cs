using MediatR;
using Sophrosync.Reporting.Application.DTOs;

namespace Sophrosync.Reporting.Application.Queries.GetNoteCompletionRate;

public sealed record GetNoteCompletionRateQuery(
    Guid TenantId,
    DateTime PeriodStart,
    DateTime PeriodEnd) : IRequest<NoteCompletionRateDto>;
