using Microsoft.EntityFrameworkCore;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Domain.Interfaces;
using Sophrosync.Notes.Infrastructure.Persistence;

namespace Sophrosync.Notes.Infrastructure.Persistence.Repositories;

public sealed class NoteRepository(NotesDbContext context) : INoteRepository
{
    public async Task<Note?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Notes.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<List<Note>> GetAllAsync(CancellationToken ct = default)
        => await context.Notes
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<Note>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default)
        => await context.Notes
            .Where(n => n.ClientId == clientId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task<Note> AddAsync(Note note, CancellationToken ct = default)
    {
        await context.Notes.AddAsync(note, ct);
        return note;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task<int> CountByStatusAsync(string status, CancellationToken ct = default)
        => await context.Notes.CountAsync(n => n.Status == status, ct);

    public async Task<int> CountByStatusGlobalAsync(string status, CancellationToken ct = default)
        => await context.Notes
            .IgnoreQueryFilters()
            .CountAsync(n => n.Status == status && !n.IsDeleted, ct);

    public async Task<Dictionary<string, int>> GetStatusCountsGlobalAsync(CancellationToken ct = default)
        => await context.Notes
            .IgnoreQueryFilters()
            .Where(n => !n.IsDeleted)
            .GroupBy(n => n.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);
}
