using MediatR;
using Sophrosync.Clients.Application.DTOs;
using Sophrosync.Clients.Domain.Interfaces;

namespace Sophrosync.Clients.Application.Commands.UpdateClient;

public sealed class UpdateClientCommandHandler(
    IClientRepository repository)
    : IRequestHandler<UpdateClientCommand, ClientDto?>
{
    public async Task<ClientDto?> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (client is null) return null;

        client.Update(request.Name, request.Email, request.Phone, request.Status);
        repository.Update(client);
        await repository.SaveChangesAsync(cancellationToken);

        return new ClientDto(client.Id, client.Name, client.Email, client.Phone, client.Status);
    }
}
