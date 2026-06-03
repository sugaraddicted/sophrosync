using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.Notes.Infrastructure.Persistence;
using Sophrosync.Notes.Infrastructure.Persistence.Repositories;

namespace Sophrosync.Notes.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var masterKey = configuration["Encryption:MasterKey"]
            ?? throw new InvalidOperationException("Encryption:MasterKey configuration value is required.");

        services.AddDbContext<NotesDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("NotesDb")));

        // Make the master key available to NotesDbContext via DI. The DbContext derives a
        // per-tenant key from this at construction time (HKDF-SHA256, tenant GUID as info)
        // so each tenant's PHI is protected by a distinct AES-256-GCM key.
        services.AddSingleton(new NotesEncryptionOptions(masterKey));

        services.AddScoped<INoteRepository, NoteRepository>();

        return services;
    }
}
