using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Currencies.DTOs;

namespace QIMy.Application.Currencies.Queries.GetCurrencyById;

public class GetCurrencyByIdQueryHandler : IRequestHandler<GetCurrencyByIdQuery, CurrencyDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCurrencyByIdQueryHandler> _logger;

    public GetCurrencyByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCurrencyByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyDto?> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Currency by Id: {Id}", request.CurrencyId);
        var Currency = await _unitOfWork.Currencies.GetByIdAsync(request.CurrencyId, cancellationToken);
        return Currency == null ? null : _mapper.Map<CurrencyDto>(Currency);
    }
}
