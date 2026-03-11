namespace Sophrosync.SharedKernel.Abstractions;

public interface ICurrentUser
{
    Guid Id { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}
