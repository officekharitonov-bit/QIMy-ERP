using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.TaxRates.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.TaxRates.Queries.GetVatRate;

public class GetVatRateQueryHandler : IRequestHandler<GetVatRateQuery, Result<VatRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetVatRateQueryHandler> _logger;

    public GetVatRateQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetVatRateQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VatRateDto>> Handle(GetVatRateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var asOfDate = request.AsOfDate ?? DateTime.UtcNow;
            var rateType = string.IsNullOrWhiteSpace(request.RateType)
                ? TaxRateType.Standard
                : Enum.Parse<TaxRateType>(request.RateType);

            _logger.LogInformation(
                "Getting VAT rate for {CountryCode}, type {RateType}, as of {Date}",
                request.CountryCode, rateType, asOfDate);

            // Find rate that was effective on the requested date
            var allRates = await _unitOfWork.TaxRates.GetAllAsync();
            var rate = allRates
                .Where(tr => tr.CountryCode == request.CountryCode)
                .Where(tr => tr.RateType == rateType)
                .Where(tr => tr.EffectiveFrom <= asOfDate)
                .Where(tr => tr.EffectiveUntil == null || tr.EffectiveUntil >= asOfDate)
                .OrderByDescending(tr => tr.EffectiveFrom)
                .FirstOrDefault();

            if (rate == null)
            {
                _logger.LogWarning(
                    "No VAT rate found for {CountryCode}, type {RateType}, as of {Date}",
                    request.CountryCode, rateType, asOfDate);

                return Result<VatRateDto>.Failure($"No VAT rate found for {request.CountryCode}");
            }

            var dto = _mapper.Map<VatRateDto>(rate);
            dto.IsActive = rate.EffectiveUntil == null;

            return Result<VatRateDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting VAT rate for {CountryCode}", request.CountryCode);
            return Result<VatRateDto>.Failure($"Error retrieving VAT rate: {ex.Message}");
        }
    }
}
