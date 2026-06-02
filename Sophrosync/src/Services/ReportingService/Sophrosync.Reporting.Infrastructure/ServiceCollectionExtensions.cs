using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sophrosync.Reporting.Application.Interfaces;
using Sophrosync.Reporting.Domain.Interfaces;
using Sophrosync.Reporting.Infrastructure.HttpClients;
using Sophrosync.Reporting.Infrastructure.Persistence;
using Sophrosync.Reporting.Infrastructure.Repositories;
using Sophrosync.SharedKernel.Http;

namespace Sophrosync.Reporting.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ReportingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ReportingDb")));

        services.AddMemoryCache();

        services.AddScoped<IReportDefinitionRepository, ReportDefinitionRepository>();
        services.AddScoped<IReportRunRepository, ReportRunRepository>();

        var policy = ResiliencePolicy.GetRetryWithCircuitBreaker();

        services.AddHttpClient<IScheduleServiceClient, ScheduleServiceClient>(client =>
                client.BaseAddress = new Uri(configuration["ServiceUrls:ScheduleService"]!))
            .AddPolicyHandler(policy);

        services.AddHttpClient<INotesServiceClient, NotesServiceClient>(client =>
                client.BaseAddress = new Uri(configuration["ServiceUrls:NotesService"]!))
            .AddPolicyHandler(policy);

        services.AddHttpClient<IClientServiceClient, ClientServiceClient>(client =>
                client.BaseAddress = new Uri(configuration["ServiceUrls:ClientService"]!))
            .AddPolicyHandler(policy);

        services.AddHttpClient<IConsentServiceClient, ConsentServiceClient>(client =>
                client.BaseAddress = new Uri(configuration["ServiceUrls:ConsentService"]!))
            .AddPolicyHandler(policy);

        return services;
    }
}
