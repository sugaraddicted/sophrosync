using MediatR;
using Sophrosync.Clients.Application.DTOs;

namespace Sophrosync.Clients.Application.Queries.GetClients;

/// <summary>
/// Returns all clients for the current tenant.
/// </summary>
public sealed record GetClientsQuery : IRequest<List<ClientDto>>;
