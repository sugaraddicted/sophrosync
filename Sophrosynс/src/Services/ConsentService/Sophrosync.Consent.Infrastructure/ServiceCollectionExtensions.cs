using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sophrosync.Consent.Domain.Interfaces;
using Sophrosync.Consent.Infrastructure.Persistence;
using Sophrosync.Consent.Infrastructure.Repositories;

namespace Sophrosync.Consent.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ConsentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ConsentDb")));

        services.AddMemoryCache();

        services.AddScoped<IConsentTemplateRepository, ConsentTemplateRepository>();
        services.AddScoped<IConsentRequestRepository, ConsentRequestRepository>();
        services.AddScoped<IConsentRecordRepository, ConsentRecordRepository>();

        return services;
    }
}
