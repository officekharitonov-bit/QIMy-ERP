using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Currencies.DTOs;

namespace QIMy.Application.Currencies.Queries.GetAllCurrencies;

public class GetAllCurrenciesQueryHandler : IRequestHandler<GetAllCurrenciesQuery, IEnumerable<CurrencyDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllCurrenciesQueryHandler> _logger;

    public GetAllCurrenciesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllCurrenciesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CurrencyDto>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all Currencies");
        var Currencies = await _unitOfWork.Currencies.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CurrencyDto>>(Currencies);
    }
}
