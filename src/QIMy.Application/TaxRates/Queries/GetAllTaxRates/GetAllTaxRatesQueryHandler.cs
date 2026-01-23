using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Queries.GetAllTaxRates;

public class GetAllTaxRatesQueryHandler : IRequestHandler<GetAllTaxRatesQuery, IEnumerable<TaxRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllTaxRatesQueryHandler> _logger;

    public GetAllTaxRatesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllTaxRatesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TaxRateDto>> Handle(GetAllTaxRatesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all tax rates");
        var taxRates = await _unitOfWork.TaxRates.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TaxRateDto>>(taxRates);
    }
}
