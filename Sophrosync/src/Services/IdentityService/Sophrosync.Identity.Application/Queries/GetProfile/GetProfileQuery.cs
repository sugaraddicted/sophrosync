using MediatR;
using Sophrosync.Identity.Application.Commands.UpdateProfile;

namespace Sophrosync.Identity.Application.Queries.GetProfile;

public sealed record GetProfileQuery : IRequest<ProfileDto>;
