using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.TaxRates.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.TaxRates.Commands.CreateTaxRate;

public class CreateTaxRateCommandHandler : IRequestHandler<CreateTaxRateCommand, Result<TaxRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTaxRateCommandHandler> _logger;

    public CreateTaxRateCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTaxRateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TaxRateDto>> Handle(CreateTaxRateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating tax rate: {Name} ({Rate}%)", request.Name, request.Rate);

        try
        {
            var existing = await _unitOfWork.TaxRates.FindAsync(t => t.Name == request.Name && !t.IsDeleted, cancellationToken);
            if (existing.Any())
                throw new DuplicateException("TaxRate", "Name", request.Name);

            var taxRate = new TaxRate
            {
                Name = request.Name,
                Rate = request.Rate,
                IsDefault = request.IsActive
            };

            await _unitOfWork.TaxRates.AddAsync(taxRate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tax rate created: Id={Id}", taxRate.Id);
            return Result<TaxRateDto>.Success(_mapper.Map<TaxRateDto>(taxRate));
        }
        catch (DuplicateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tax rate");
            return Result<TaxRateDto>.Failure($"Ошибка создания: {ex.Message}");
        }
    }
}
