using MediatR;
using Sophrosync.Clients.Domain.Interfaces;

namespace Sophrosync.Clients.Application.Commands.DeleteClient;

public sealed class DeleteClientCommandHandler(
    IClientRepository repository)
    : IRequestHandler<DeleteClientCommand, bool>
{
    public async Task<bool> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var client = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (client is null) return false;

        client.SoftDelete();
        repository.Update(client);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
