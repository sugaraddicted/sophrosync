using MediatR;

namespace Sophrosync.Clients.Application.Commands.DeleteClient;

/// <summary>
/// Permanently removes a client record from the current tenant's scope.
/// </summary>
public sealed record DeleteClientCommand(Guid Id) : IRequest<bool>;
