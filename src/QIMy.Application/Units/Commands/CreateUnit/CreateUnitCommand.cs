using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.Units.DTOs;

namespace QIMy.Application.Units.Commands.CreateUnit;

public record CreateUnitCommand : IRequest<Result<UnitDto>>
{
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public bool IsDefault { get; init; } = false;
}
