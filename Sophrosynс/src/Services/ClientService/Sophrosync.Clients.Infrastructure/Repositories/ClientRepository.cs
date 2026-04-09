using Microsoft.EntityFrameworkCore;
using Sophrosync.Clients.Domain.Entities;
using Sophrosync.Clients.Domain.Interfaces;
using Sophrosync.Clients.Infrastructure.Persistence;

namespace Sophrosync.Clients.Infrastructure.Repositories;

public sealed class ClientRepository(ClientsDbContext context) : IClientRepository
{
    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Client entity, CancellationToken cancellationToken = default)
        => await context.Clients.AddAsync(entity, cancellationToken);

    public void Update(Client entity)
        => context.Clients.Update(entity);

    public void Remove(Client entity)
        => context.Clients.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken ct = default)
        => await context.Clients
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
}
