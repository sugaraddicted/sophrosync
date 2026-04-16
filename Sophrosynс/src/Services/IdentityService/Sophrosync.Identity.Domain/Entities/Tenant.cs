using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Identity.Domain.Entities;

public sealed class Tenant : Entity
{
    public string Name { get; private set; } = null!;
    public string TimeZone { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Tenant() { }

    public static Tenant Create(string name, string timeZone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZone);
        return new Tenant { Name = name, TimeZone = timeZone, IsActive = true };
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
