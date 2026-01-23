using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Currencies.Commands.DeleteCurrency;

public class DeleteCurrencyCommandHandler : IRequestHandler<DeleteCurrencyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCurrencyCommandHandler> _logger;

    public DeleteCurrencyCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteCurrencyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Currency: Id={Id}", request.CurrencyId);

        try
        {
            var exists = await _unitOfWork.Currencies.ExistsAsync(request.CurrencyId, cancellationToken);
            if (!exists)
                throw new NotFoundException("Currency", request.CurrencyId);

            await _unitOfWork.Currencies.DeleteAsync(request.CurrencyId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Currency deleted: Id={Id}", request.CurrencyId);
            return Result.Success();
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Currency");
            return Result.Failure($"Ошибка удаления: {ex.Message}");
        }
    }
}
