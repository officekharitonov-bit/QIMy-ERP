using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Core.Entities;
using Unit = QIMy.Core.Entities.Unit;

namespace QIMy.Application.Units.Commands.DeleteUnit;

public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteUnitCommandHandler> _logger;

    public DeleteUnitCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteUnitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Unit: Id={Id}", request.UnitId);

        try
        {
            var exists = await _unitOfWork.Units.ExistsAsync(request.UnitId, cancellationToken);
            if (!exists)
                throw new NotFoundException("Unit", request.UnitId);

            await _unitOfWork.Units.DeleteAsync(request.UnitId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Unit deleted: Id={Id}", request.UnitId);
            return Result.Success();
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Unit");
            return Result.Failure($"Ошибка удаления: {ex.Message}");
        }
    }
}
