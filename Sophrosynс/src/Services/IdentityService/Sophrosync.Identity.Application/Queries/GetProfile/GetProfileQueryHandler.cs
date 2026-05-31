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
            .FirstOrDefaultAsync(p => p.KeycloakUserId == userId, ct)
            ?? throw new KeyNotFoundException($"User profile not found for user {userId}.");

        return new ProfileDto(profile.FirstName, profile.LastName, profile.Email, profile.Role);
    }
}
