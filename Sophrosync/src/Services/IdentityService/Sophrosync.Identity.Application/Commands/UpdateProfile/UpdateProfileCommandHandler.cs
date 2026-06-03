using MediatR;
using Microsoft.EntityFrameworkCore;
using Sophrosync.Identity.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Identity.Application.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    private readonly IIdentityDbContext _db;
    private readonly IKeycloakAdminService _keycloak;
    private readonly ICurrentUser _currentUser;

    public UpdateProfileCommandHandler(
        IIdentityDbContext db,
        IKeycloakAdminService keycloak,
        ICurrentUser currentUser)
    {
        _db = db;
        _keycloak = keycloak;
        _currentUser = currentUser;
    }

    public async Task<ProfileDto> Handle(UpdateProfileCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.Id;

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.KeycloakUserId == userId, ct)
            ?? throw new KeyNotFoundException($"User profile not found for user {userId}.");

        await _keycloak.UpdateUserAsync(userId, cmd.FirstName, cmd.LastName, ct);

        profile.UpdateName(cmd.FirstName, cmd.LastName);
        await _db.SaveChangesAsync(ct);

        return new ProfileDto(profile.FirstName, profile.LastName, profile.Email, profile.Role);
    }
}
