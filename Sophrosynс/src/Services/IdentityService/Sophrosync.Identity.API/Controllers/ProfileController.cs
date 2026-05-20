using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Identity.Application.Queries.GetCurrentUser;

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
}
