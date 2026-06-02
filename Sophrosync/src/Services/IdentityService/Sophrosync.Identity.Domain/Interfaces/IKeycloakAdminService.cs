namespace Sophrosync.Identity.Domain.Interfaces;

public interface IKeycloakAdminService
{
    Task<Guid> CreateUserAsync(CreateKeycloakUserRequest request, CancellationToken ct);
    Task DeleteUserAsync(Guid keycloakUserId, CancellationToken ct);
    Task UpdateUserAsync(Guid keycloakUserId, string firstName, string lastName, CancellationToken ct);
    Task<(string Email, string FirstName, string LastName)> GetUserAsync(Guid keycloakUserId, CancellationToken ct);
}

public record CreateKeycloakUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid TenantId
);
