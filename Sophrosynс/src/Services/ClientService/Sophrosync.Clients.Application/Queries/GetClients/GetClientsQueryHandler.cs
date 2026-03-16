using MediatR;
using Sophrosync.Clients.Application.DTOs;
using Sophrosync.Clients.Domain.Interfaces;

namespace Sophrosync.Clients.Application.Queries.GetClients;

public sealed class GetClientsQueryHandler(
    IClientRepository repository)
    : IRequestHandler<GetClientsQuery, List<ClientDto>>
{
    public async Task<List<ClientDto>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var clients = await repository.GetAllAsync(cancellationToken);

        return clients
            .Select(c => new ClientDto(c.Id, c.Name, c.Email, c.Phone, c.Status))
            .ToList();
    }
}
