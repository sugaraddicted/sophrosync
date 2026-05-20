using MediatR;
using Sophrosync.Clients.Application.DTOs;
using Sophrosync.Clients.Domain.Interfaces;

namespace Sophrosync.Clients.Application.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(
    IClientRepository repository)
    : IRequestHandler<GetClientByIdQuery, ClientDto?>
{
    public async Task<ClientDto?> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (client is null) return null;

        return new ClientDto(client.Id, client.Name, client.Email, client.Phone, client.Status);
    }
}
