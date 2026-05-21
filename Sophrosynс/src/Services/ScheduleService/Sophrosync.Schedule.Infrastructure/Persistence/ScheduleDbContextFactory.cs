using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Infrastructure.Persistence;

public sealed class ScheduleDbContextFactory : IDesignTimeDbContextFactory<ScheduleDbContext>
{
    public ScheduleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ScheduleDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ScheduleDb")
            ?? "Host=localhost;Port=5432;Database=sophrosync_schedule;Username=svc_schedule;Password=schedule_dev_pw";

        optionsBuilder.UseNpgsql(connectionString);

        return new ScheduleDbContext(optionsBuilder.Options, new DesignTimeTenant());
    }

    private sealed class DesignTimeTenant : ICurrentTenant
    {
        public Guid Id { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public bool HasTenant => false;
    }
}
