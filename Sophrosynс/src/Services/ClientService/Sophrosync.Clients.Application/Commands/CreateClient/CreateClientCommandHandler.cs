using MediatR;
using Sophrosync.Clients.Application.DTOs;
using Sophrosync.Clients.Domain.Entities;
using Sophrosync.Clients.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Clients.Application.Commands.CreateClient;

public sealed class CreateClientCommandHandler(
    IClientRepository repository,
    ICurrentTenant currentTenant)
    : IRequestHandler<CreateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var client = Client.Create(
            currentTenant.Id,
            request.Name,
            request.Email,
            request.Phone);

        await repository.AddAsync(client, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new ClientDto(client.Id, client.Name, client.Email, client.Phone, client.Status);
    }
}
