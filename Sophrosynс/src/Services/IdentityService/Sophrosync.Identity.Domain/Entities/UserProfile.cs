using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Identity.Domain.Entities;

public sealed class UserProfile : Entity
{
    public Guid TenantId { get; private set; }
    public Guid KeycloakUserId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Role { get; private set; } = null!;
    public DateTime AcceptedTermsAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private UserProfile() { }

    public static UserProfile Create(
        Guid tenantId,
        Guid keycloakUserId,
        string firstName,
        string lastName,
        string email,
        DateTime acceptedTermsAt)
    {
        return new UserProfile
        {
            TenantId = tenantId,
            KeycloakUserId = keycloakUserId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = "therapist",
            AcceptedTermsAt = acceptedTermsAt
        };
    }
}
