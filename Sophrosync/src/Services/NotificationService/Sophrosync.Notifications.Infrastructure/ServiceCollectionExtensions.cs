using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sophrosync.Notifications.Application.Interfaces;
using Sophrosync.Notifications.Domain.Interfaces;
using Sophrosync.Notifications.Infrastructure.Channels;
using Sophrosync.Notifications.Infrastructure.Persistence;
using Sophrosync.Notifications.Infrastructure.Repositories;

namespace Sophrosync.Notifications.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("NotificationsDb")));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();

        services.AddSingleton<INotificationChannel, InAppNotificationChannel>();
        services.AddSingleton<INotificationChannel, EmailNotificationChannel>();

        return services;
    }
}
