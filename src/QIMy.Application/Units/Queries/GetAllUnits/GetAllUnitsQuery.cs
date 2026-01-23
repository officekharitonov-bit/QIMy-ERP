using MediatR;
using QIMy.Application.Units.DTOs;

namespace QIMy.Application.Units.Queries.GetAllUnits;

public record GetAllUnitsQuery : IRequest<IEnumerable<UnitDto>>;
