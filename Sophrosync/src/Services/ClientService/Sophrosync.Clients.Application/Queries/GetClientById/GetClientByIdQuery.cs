using MediatR;
using Sophrosync.Clients.Application.DTOs;

namespace Sophrosync.Clients.Application.Queries.GetClientById;

/// <summary>
/// Returns a single client by ID, scoped to the current tenant.
/// </summary>
public sealed record GetClientByIdQuery(Guid Id) : IRequest<ClientDto?>;
