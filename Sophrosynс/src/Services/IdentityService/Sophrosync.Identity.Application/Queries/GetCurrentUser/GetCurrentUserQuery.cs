using MediatR;

namespace Sophrosync.Identity.Application.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<CurrentUserDto>;

public record CurrentUserDto(
    Guid TenantId,
    string FirstName,
    string LastName,
    string Email,
    string PracticeName,
    string Role
);
