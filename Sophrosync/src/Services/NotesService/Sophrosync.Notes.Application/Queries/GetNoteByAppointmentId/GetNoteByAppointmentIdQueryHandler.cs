using MediatR;
using Sophrosync.Notes.Application.DTOs;
using Sophrosync.Notes.Domain.Interfaces;

namespace Sophrosync.Notes.Application.Queries.GetNoteByAppointmentId;

public sealed class GetNoteByAppointmentIdQueryHandler(INoteRepository repository)
    : IRequestHandler<GetNoteByAppointmentIdQuery, NoteDto?>
{
    public async Task<NoteDto?> Handle(GetNoteByAppointmentIdQuery request, CancellationToken cancellationToken)
    {
        var note = await repository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
        return note is null ? null : NoteDto.FromNote(note);
    }
}
