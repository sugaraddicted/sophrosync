using MediatR;
using Sophrosync.Reporting.Application.DTOs;

namespace Sophrosync.Reporting.Application.Queries.GetGdprRoPA;

public sealed record GetGdprRoPAQuery : IRequest<IReadOnlyList<GdprRoPAEntryDto>>;
