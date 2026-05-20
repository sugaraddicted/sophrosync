namespace Sophrosync.Clients.Application.DTOs;

/// <summary>
/// Read model returned to callers for a single client.
/// </summary>
public record ClientDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string Status);
