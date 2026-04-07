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
        var encryptionKey = configuration["Encryption:NotesKey"]
            ?? throw new InvalidOperationException("Encryption:NotesKey configuration value is required.");

        services.AddDbContext<NotesDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("NotesDb")));

        // Make the encryption key available to NotesDbContext via DI so it can construct
        // NoteConfiguration(encryptionKey) explicitly rather than relying on
        // ApplyConfigurationsFromAssembly (which calls the parameterless constructor with placeholder key).
        services.AddSingleton(new NotesEncryptionOptions(encryptionKey));

        services.AddScoped<INoteRepository, NoteRepository>();

        return services;
    }
}
