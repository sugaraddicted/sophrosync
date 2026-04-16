using MediatR;

namespace Sophrosync.Identity.Application.Commands.RegisterPractice;

public record RegisterPracticeCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PracticeName,
    string TimeZone,
    bool AcceptedTerms
) : IRequest<RegisterPracticeResult>;

public record RegisterPracticeResult(string Message);
