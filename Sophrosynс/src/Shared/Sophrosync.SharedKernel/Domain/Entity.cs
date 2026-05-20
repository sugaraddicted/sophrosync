namespace Sophrosync.SharedKernel.Domain;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the <see cref="UpdatedAt"/> timestamp.
    /// Called by the DbContext <c>SaveChangesAsync</c> override before persisting changes.
    /// </summary>
    public void TouchUpdatedAt(DateTime utcNow)
    {
        UpdatedAt = utcNow;
    }
}
