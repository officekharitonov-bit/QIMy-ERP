using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Currencies.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.Currencies.Commands.CreateCurrency;

public class CreateCurrencyCommandHandler : IRequestHandler<CreateCurrencyCommand, Result<CurrencyDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCurrencyCommandHandler> _logger;

    public CreateCurrencyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateCurrencyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CurrencyDto>> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Currency: {Name} ({Rate}%)", request.Name, request.ExchangeRate);

        try
        {
            var existing = await _unitOfWork.Currencies.FindAsync(t => t.Name == request.Name && !t.IsDeleted, cancellationToken);
            if (existing.Any())
                throw new DuplicateException("Currency", "Name", request.Name);

            var Currency = new Currency
            {
                Name = request.Name,
                ExchangeRate = request.ExchangeRate,
                IsDefault = request.IsDefault
            };

            await _unitOfWork.Currencies.AddAsync(Currency, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Currency created: Id={Id}", Currency.Id);
            return Result<CurrencyDto>.Success(_mapper.Map<CurrencyDto>(Currency));
        }
        catch (DuplicateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Currency");
            return Result<CurrencyDto>.Failure($"Ошибка создания: {ex.Message}");
        }
    }
}
