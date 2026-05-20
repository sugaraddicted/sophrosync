using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sophrosync.Identity.Application.Commands.RegisterPractice;

namespace Sophrosync.Identity.API.Controllers;

[ApiController]
[Route("api/identity")]
[AllowAnonymous]
public sealed class RegistrationController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPracticeRequest request,
        [FromServices] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new RegisterPracticeCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PracticeName,
            request.TimeZone,
            request.AcceptedTerms), ct);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}

public record RegisterPracticeRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PracticeName,
    string TimeZone,
    bool AcceptedTerms);
