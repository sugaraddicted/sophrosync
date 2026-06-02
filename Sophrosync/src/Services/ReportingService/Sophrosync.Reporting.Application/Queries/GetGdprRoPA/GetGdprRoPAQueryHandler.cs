using MediatR;
using Sophrosync.Reporting.Application.Configuration;
using Sophrosync.Reporting.Application.DTOs;

namespace Sophrosync.Reporting.Application.Queries.GetGdprRoPA;

public sealed class GetGdprRoPAQueryHandler : IRequestHandler<GetGdprRoPAQuery, IReadOnlyList<GdprRoPAEntryDto>>
{
    public Task<IReadOnlyList<GdprRoPAEntryDto>> Handle(GetGdprRoPAQuery request, CancellationToken cancellationToken)
        => Task.FromResult(GdprRoPAConfiguration.GetEntries());
}
