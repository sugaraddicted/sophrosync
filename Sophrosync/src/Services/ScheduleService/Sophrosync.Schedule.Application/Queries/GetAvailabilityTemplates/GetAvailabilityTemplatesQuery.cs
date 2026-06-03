using MediatR;
using Sophrosync.Schedule.Application.DTOs;

namespace Sophrosync.Schedule.Application.Queries.GetAvailabilityTemplates;

/// <summary>Returns all availability templates for the requesting therapist.</summary>
public sealed record GetAvailabilityTemplatesQuery() : IRequest<IReadOnlyList<AvailabilityTemplateDto>>;
