namespace Sophrosync.Identity.Domain.Interfaces;

public interface IKeycloakAdminService
{
    Task<Guid> CreateUserAsync(CreateKeycloakUserRequest request, CancellationToken ct);
    Task DeleteUserAsync(Guid keycloakUserId, CancellationToken ct);
}

public record CreateKeycloakUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid TenantId
);
