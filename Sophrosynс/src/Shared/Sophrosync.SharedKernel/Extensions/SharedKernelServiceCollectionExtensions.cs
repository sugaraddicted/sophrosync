using Microsoft.Extensions.DependencyInjection;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Services;

namespace Sophrosync.SharedKernel.Extensions;

/// <summary>
/// DI registration for SharedKernel cross-cutting services.
/// Per spec Section 5 (SharedKernel): ICurrentTenant and ICurrentUser resolved from HTTP context.
/// </summary>
public static class SharedKernelServiceCollectionExtensions
{
    /// <summary>
    /// Registers IHttpContextAccessor, ICurrentTenant (CurrentTenantService),
    /// and ICurrentUser (CurrentUserService) as scoped services.
    /// Call from each service's Program.cs before AddMediatR / AddControllers.
    /// </summary>
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenant, CurrentTenantService>();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        return services;
    }
}
