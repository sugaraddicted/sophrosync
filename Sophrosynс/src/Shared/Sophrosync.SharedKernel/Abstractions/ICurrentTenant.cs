namespace Sophrosync.SharedKernel.Abstractions;

public interface ICurrentTenant
{
    Guid Id { get; }
    bool HasTenant { get; }
}
