using MediatR;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Units.Commands.DeleteUnit;

public record DeleteUnitCommand(int UnitId) : IRequest<Result>;
