using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Notes.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used exclusively by EF Core tooling (migrations, scaffolding).
/// Provides a stub <see cref="ICurrentTenant"/> so the DbContext can be instantiated
/// without a live HTTP request context.
/// Never used at runtime — runtime DI uses the registered <see cref="ICurrentTenant"/> service.
/// </summary>
public sealed class NotesDbContextFactory : IDesignTimeDbContextFactory<NotesDbContext>
{
    public NotesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotesDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__NotesDb")
            ?? "Host=localhost;Port=5432;Database=sophrosync_notes;Username=svc_notes;Password=notes_dev_pw";

        optionsBuilder.UseNpgsql(connectionString);

        var encKey = Environment.GetEnvironmentVariable("Encryption__NotesKey")
            ?? "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

        return new NotesDbContext(
            optionsBuilder.Options,
            new DesignTimeTenant(),
            new DesignTimeUser(),
            new NotesEncryptionOptions(encKey));
    }

    /// <summary>Stub tenant for EF Core design-time tooling.</summary>
    private sealed class DesignTimeTenant : ICurrentTenant
    {
        public Guid Id { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public bool HasTenant => false;
    }

    /// <summary>Stub user for EF Core design-time tooling. Has no roles so the therapist filter is not applied.</summary>
    private sealed class DesignTimeUser : ICurrentUser
    {
        public Guid Id { get; } = Guid.Empty;
        public string? Email => null;
        public string FullName => "design-time";
        public IReadOnlyList<string> Roles => [];
        public bool IsInRole(string role) => false;
    }
}
