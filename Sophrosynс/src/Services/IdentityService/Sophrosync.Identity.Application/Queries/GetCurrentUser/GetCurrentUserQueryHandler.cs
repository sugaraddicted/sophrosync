using MediatR;
using Microsoft.EntityFrameworkCore;
using Sophrosync.Identity.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Identity.Application.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IIdentityDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserQueryHandler(IIdentityDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery _, CancellationToken ct)
    {
        var email = _currentUser.Email
            ?? throw new UnauthorizedAccessException("No email claim in token.");

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.Email == email, ct)
            ?? throw new KeyNotFoundException("User profile not found.");

        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Id == profile.TenantId, ct)
            ?? throw new KeyNotFoundException("Tenant not found.");

        return new CurrentUserDto(
            profile.TenantId,
            profile.FirstName,
            profile.LastName,
            profile.Email,
            tenant.Name,
            profile.Role);
    }
}
