using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sophrosync.Identity.Domain.Interfaces;
using Sophrosync.Identity.Infrastructure.Persistence;
using Sophrosync.Identity.Infrastructure.Services;

namespace Sophrosync.Identity.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("IdentityDb")));

        services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<IdentityDbContext>());

        services.AddHttpClient("keycloak-admin", client =>
        {
            var baseUrl = configuration["Keycloak:BaseUrl"]
                ?? throw new InvalidOperationException("Keycloak:BaseUrl is required.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();

        return services;
    }
}
