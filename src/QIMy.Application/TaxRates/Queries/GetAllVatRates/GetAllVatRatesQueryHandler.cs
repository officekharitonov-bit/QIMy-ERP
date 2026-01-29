using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Queries.GetAllVatRates;

public class GetAllVatRatesQueryHandler : IRequestHandler<GetAllVatRatesQuery, Result<List<VatRateDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllVatRatesQueryHandler> _logger;

    public GetAllVatRatesQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetAllVatRatesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<VatRateDto>>> Handle(GetAllVatRatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting all VAT rates (IncludeHistorical: {Include})", request.IncludeHistorical);

            var allRates = await _unitOfWork.TaxRates.GetAllAsync();

            var query = allRates.AsEnumerable();

            // Filter by country if specified
            if (!string.IsNullOrWhiteSpace(request.CountryCode))
            {
                query = query.Where(tr => tr.CountryCode == request.CountryCode);
            }

            // Filter active only if not including historical
            if (!request.IncludeHistorical)
            {
                query = query.Where(tr => tr.EffectiveUntil == null);
            }

            var rates = query
                .OrderBy(tr => tr.CountryCode)
                .ThenBy(tr => tr.RateType)
                .ThenByDescending(tr => tr.EffectiveFrom)
                .ToList();

            var dtos = rates.Select(rate =>
            {
                var dto = _mapper.Map<VatRateDto>(rate);
                dto.IsActive = rate.EffectiveUntil == null;
                return dto;
            }).ToList();

            _logger.LogInformation("Found {Count} VAT rates", dtos.Count);

            return Result<List<VatRateDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all VAT rates");
            return Result<List<VatRateDto>>.Failure($"Error retrieving VAT rates: {ex.Message}");
        }
    }
}
