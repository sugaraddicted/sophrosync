using MediatR;
using Sophrosync.Clients.Application.DTOs;

namespace Sophrosync.Clients.Application.Commands.CreateClient;

/// <summary>
/// Creates a new client within the current tenant.
/// TenantId is set by the handler from ICurrentTenant — never supplied by the caller.
/// </summary>
public sealed record CreateClientCommand(
    string Name,
    string Email,
    string Phone) : IRequest<ClientDto>;
