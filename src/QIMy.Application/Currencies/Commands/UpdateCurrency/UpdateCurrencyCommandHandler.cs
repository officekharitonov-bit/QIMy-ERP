using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Currencies.DTOs;

namespace QIMy.Application.Currencies.Commands.UpdateCurrency;

public class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, Result<CurrencyDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCurrencyCommandHandler> _logger;

    public UpdateCurrencyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateCurrencyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CurrencyDto>> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Currency: Id={Id}", request.Id);

        try
        {
            var Currency = await _unitOfWork.Currencies.GetByIdAsync(request.Id, cancellationToken);
            if (Currency == null)
                throw new NotFoundException("Currency", request.Id);

            var duplicate = await _unitOfWork.Currencies.FindAsync(
                t => t.Name == request.Name && t.Id != request.Id && !t.IsDeleted, cancellationToken);
            if (duplicate.Any())
                throw new DuplicateException("Currency", "Name", request.Name);

            Currency.Name = request.Name;
            Currency.ExchangeRate = request.ExchangeRate;
            Currency.IsDefault = request.IsDefault;

            await _unitOfWork.Currencies.UpdateAsync(Currency, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Currency updated: Id={Id}", Currency.Id);
            return Result<CurrencyDto>.Success(_mapper.Map<CurrencyDto>(Currency));
        }
        catch (NotFoundException) { throw; }
        catch (DuplicateException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Currency");
            return Result<CurrencyDto>.Failure($"Ошибка обновления: {ex.Message}");
        }
    }
}
