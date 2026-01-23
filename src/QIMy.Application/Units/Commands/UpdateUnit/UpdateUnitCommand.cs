using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.Units.DTOs;

namespace QIMy.Application.Units.Commands.UpdateUnit;

public record UpdateUnitCommand : IRequest<Result<UnitDto>>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}
