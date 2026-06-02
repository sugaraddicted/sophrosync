using Microsoft.EntityFrameworkCore;
using Sophrosync.Schedule.Domain.Entities;
using Sophrosync.Schedule.Domain.Interfaces;
using Sophrosync.Schedule.Infrastructure.Persistence;

namespace Sophrosync.Schedule.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository(ScheduleDbContext context) : IAppointmentRepository
{
    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken ct = default)
        => await context.Appointments
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default)
        => await context.Appointments
            .Where(a => a.ClientId == clientId)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetByTherapistIdAsync(Guid therapistId, CancellationToken ct = default)
        => await context.Appointments
            .Where(a => a.TherapistId == therapistId)
            .OrderByDescending(a => a.ScheduledAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Appointment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await context.Appointments
            .Where(a => a.ScheduledAt >= from && a.ScheduledAt <= to)
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync(ct);

    public async Task AddAsync(Appointment entity, CancellationToken cancellationToken = default)
        => await context.Appointments.AddAsync(entity, cancellationToken);

    public void Update(Appointment entity)
        => context.Appointments.Update(entity);

    public void Remove(Appointment entity)
        => context.Appointments.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
