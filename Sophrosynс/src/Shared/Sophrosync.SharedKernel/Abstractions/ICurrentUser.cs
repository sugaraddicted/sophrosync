namespace Sophrosync.SharedKernel.Abstractions;

public interface ICurrentUser
{
    Guid Id { get; }
    string? Email { get; }
    string FullName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);
}
