using MediatR;
using Microsoft.EntityFrameworkCore;
using Sophrosync.Identity.Application.Commands.UpdateProfile;
using Sophrosync.Identity.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Identity.Application.Queries.GetProfile;

public sealed class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileDto>
{
    private readonly IIdentityDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetProfileQueryHandler(IIdentityDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProfileDto> Handle(GetProfileQuery _, CancellationToken ct)
    {
        var userId = _currentUser.Id;

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.KeycloakUserId == userId, ct);

        if (profile is not null)
            return new ProfileDto(profile.FirstName, profile.LastName, profile.Email, profile.Role);

        // User exists in Keycloak but has no DB profile (e.g. admin-created accounts).
        // Return data from JWT claims so the profile page still loads.
        var nameParts = _currentUser.FullName.Split(' ', 2);
        var role = _currentUser.IsInRole("admin") ? "admin"
            : _currentUser.IsInRole("supervisor") ? "supervisor"
            : "therapist";

        return new ProfileDto(
            nameParts[0],
            nameParts.Length > 1 ? nameParts[1] : string.Empty,
            _currentUser.Email ?? string.Empty,
            role);
    }
}
