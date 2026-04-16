using MediatR;
using Sophrosync.Identity.Domain.Entities;
using Sophrosync.Identity.Domain.Interfaces;

namespace Sophrosync.Identity.Application.Commands.RegisterPractice;

public sealed class RegisterPracticeCommandHandler : IRequestHandler<RegisterPracticeCommand, RegisterPracticeResult>
{
    private readonly IIdentityDbContext _db;
    private readonly IKeycloakAdminService _keycloak;

    public RegisterPracticeCommandHandler(IIdentityDbContext db, IKeycloakAdminService keycloak)
    {
        _db = db;
        _keycloak = keycloak;
    }

    public async Task<RegisterPracticeResult> Handle(RegisterPracticeCommand cmd, CancellationToken ct)
    {
        var tenant = Tenant.Create(cmd.PracticeName, cmd.TimeZone);

        // Step 1: create Keycloak user first (email-uniqueness check happens here)
        var keycloakUserId = await _keycloak.CreateUserAsync(
            new CreateKeycloakUserRequest(cmd.Email, cmd.Password, cmd.FirstName, cmd.LastName, tenant.Id), ct);

        // Step 2: persist to DB — single save, with correct KeycloakUserId from the start
        var profile = UserProfile.Create(
            tenant.Id, keycloakUserId, cmd.FirstName, cmd.LastName, cmd.Email, DateTime.UtcNow);

        try
        {
            await _db.Tenants.AddAsync(tenant, ct);
            await _db.UserProfiles.AddAsync(profile, ct);
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            // Compensate: remove the Keycloak user so the email is not permanently locked
            await _keycloak.DeleteUserAsync(keycloakUserId, CancellationToken.None);
            throw;
        }

        return new RegisterPracticeResult("Registration successful. You can now sign in.");
    }
}
