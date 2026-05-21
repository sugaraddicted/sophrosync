using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.Schedule.Infrastructure.Persistence;
using Sophrosync.Schedule.Infrastructure.Persistence.Repositories;

namespace Sophrosync.Schedule.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScheduleInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ScheduleDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("ScheduleDb")));

        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        return services;
    }
}
