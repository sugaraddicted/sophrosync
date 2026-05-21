using Sophrosync.Schedule.Domain.Entities;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Schedule.Domain.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByTherapistIdAsync(Guid therapistId, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
