using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Commands.UpdateTaxRate;

public class UpdateTaxRateCommandHandler : IRequestHandler<UpdateTaxRateCommand, Result<TaxRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateTaxRateCommandHandler> _logger;

    public UpdateTaxRateCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTaxRateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TaxRateDto>> Handle(UpdateTaxRateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating tax rate: Id={Id}", request.Id);

        try
        {
            var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(request.Id, cancellationToken);
            if (taxRate == null)
                throw new NotFoundException("TaxRate", request.Id);

            var duplicate = await _unitOfWork.TaxRates.FindAsync(
                t => t.Name == request.Name && t.Id != request.Id && !t.IsDeleted, cancellationToken);
            if (duplicate.Any())
                throw new DuplicateException("TaxRate", "Name", request.Name);

            taxRate.Name = request.Name;
            taxRate.Rate = request.Rate;
            taxRate.IsDefault = request.IsActive;

            await _unitOfWork.TaxRates.UpdateAsync(taxRate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tax rate updated: Id={Id}", taxRate.Id);
            return Result<TaxRateDto>.Success(_mapper.Map<TaxRateDto>(taxRate));
        }
        catch (NotFoundException) { throw; }
        catch (DuplicateException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tax rate");
            return Result<TaxRateDto>.Failure($"Ошибка обновления: {ex.Message}");
        }
    }
}
