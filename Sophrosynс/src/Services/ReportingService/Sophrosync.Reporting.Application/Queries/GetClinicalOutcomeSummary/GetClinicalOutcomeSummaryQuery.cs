using MediatR;
using Sophrosync.Reporting.Application.DTOs;

namespace Sophrosync.Reporting.Application.Queries.GetClinicalOutcomeSummary;

public sealed record GetClinicalOutcomeSummaryQuery(
    Guid TenantId,
    DateTime PeriodStart,
    DateTime PeriodEnd) : IRequest<ClinicalOutcomeSummaryDto>;
