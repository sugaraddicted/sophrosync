using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Identity.Application.Commands.UpdateProfile;
using Sophrosync.Identity.Application.Queries.GetCurrentUser;
using Sophrosync.Identity.Application.Queries.GetProfile;

namespace Sophrosync.Identity.API.Controllers;

[ApiController]
[Route("api/identity")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> GetMe(
        [FromServices] ISender sender,
        CancellationToken ct)
        => Ok(await sender.Send(new GetCurrentUserQuery(), ct));

    [HttpGet("profile")]
    public async Task<ActionResult<ProfileDto>> GetProfile(
        [FromServices] ISender sender,
        CancellationToken ct)
        => Ok(await sender.Send(new GetProfileQuery(), ct));

    [HttpPut("profile")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        [FromServices] ISender sender,
        CancellationToken ct)
        => Ok(await sender.Send(
            new UpdateProfileCommand(request.FirstName, request.LastName), ct));
}

public record UpdateProfileRequest(string FirstName, string LastName);
