using MediatR;
using QIMy.Application.Units.DTOs;

namespace QIMy.Application.Units.Queries.GetUnitById;

public record GetUnitByIdQuery(int UnitId) : IRequest<UnitDto?>;
