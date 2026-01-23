using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;

namespace QIMy.Application.TaxRates.Commands.DeleteTaxRate;

public class DeleteTaxRateCommandHandler : IRequestHandler<DeleteTaxRateCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTaxRateCommandHandler> _logger;

    public DeleteTaxRateCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteTaxRateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTaxRateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting tax rate: Id={Id}", request.TaxRateId);

        try
        {
            var exists = await _unitOfWork.TaxRates.ExistsAsync(request.TaxRateId, cancellationToken);
            if (!exists)
                throw new NotFoundException("TaxRate", request.TaxRateId);

            await _unitOfWork.TaxRates.DeleteAsync(request.TaxRateId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tax rate deleted: Id={Id}", request.TaxRateId);
            return Result.Success();
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tax rate");
            return Result.Failure($"Ошибка удаления: {ex.Message}");
        }
    }
}
