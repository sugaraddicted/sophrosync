using MediatR;
using Sophrosync.Clients.Application.DTOs;

namespace Sophrosync.Clients.Application.Commands.UpdateClient;

/// <summary>
/// Updates an existing client's mutable fields.
/// </summary>
public sealed record UpdateClientCommand(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string Status) : IRequest<ClientDto?>;
