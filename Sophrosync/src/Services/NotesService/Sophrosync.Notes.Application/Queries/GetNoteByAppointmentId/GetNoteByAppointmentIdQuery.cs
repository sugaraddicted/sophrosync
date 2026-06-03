using MediatR;
using Sophrosync.Notes.Application.DTOs;

namespace Sophrosync.Notes.Application.Queries.GetNoteByAppointmentId;

public sealed record GetNoteByAppointmentIdQuery(Guid AppointmentId) : IRequest<NoteDto?>;
