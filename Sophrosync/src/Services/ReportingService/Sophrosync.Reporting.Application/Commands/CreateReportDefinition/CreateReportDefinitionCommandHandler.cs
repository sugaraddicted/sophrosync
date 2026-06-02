using MediatR;
using Sophrosync.Reporting.Domain.Entities;
using Sophrosync.Reporting.Domain.Interfaces;

namespace Sophrosync.Reporting.Application.Commands.CreateReportDefinition;

public sealed class CreateReportDefinitionCommandHandler(
    IReportDefinitionRepository repository) : IRequestHandler<CreateReportDefinitionCommand, Guid>
{
    public async Task<Guid> Handle(CreateReportDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = ReportDefinition.Create(
            request.TenantId, request.Name, request.Type, request.Format, request.Schedule);
        await repository.AddAsync(definition, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return definition.Id;
    }
}
