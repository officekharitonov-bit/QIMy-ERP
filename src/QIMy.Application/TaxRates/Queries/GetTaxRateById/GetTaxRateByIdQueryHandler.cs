using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Queries.GetTaxRateById;

public class GetTaxRateByIdQueryHandler : IRequestHandler<GetTaxRateByIdQuery, TaxRateDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTaxRateByIdQueryHandler> _logger;

    public GetTaxRateByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTaxRateByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TaxRateDto?> Handle(GetTaxRateByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tax rate by Id: {Id}", request.TaxRateId);
        var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(request.TaxRateId, cancellationToken);
        return taxRate == null ? null : _mapper.Map<TaxRateDto>(taxRate);
    }
}
