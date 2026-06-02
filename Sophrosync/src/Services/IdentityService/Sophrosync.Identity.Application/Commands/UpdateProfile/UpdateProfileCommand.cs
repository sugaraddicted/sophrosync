using MediatR;

namespace Sophrosync.Identity.Application.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(string FirstName, string LastName)
    : IRequest<ProfileDto>;

public sealed record ProfileDto(string FirstName, string LastName, string Email, string Role);
