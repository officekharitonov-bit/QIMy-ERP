using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Accounts.DTOs;
using QIMy.Application.Common.Interfaces;

namespace QIMy.Application.Accounts.Queries.GetAccountById;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAccountByIdQueryHandler> _logger;

    public GetAccountByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAccountByIdQueryHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._logger = _logger;
    }

    public async Task<AccountDto?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting account by ID: {AccountId}", request.AccountId);

        var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Account not found: {AccountId}", request.AccountId);
            return null;
        }

        var dto = _mapper.Map<AccountDto>(account);

        if (account.DefaultTaxRate != null)
        {
            dto = dto with
            {
                TaxRateName = account.DefaultTaxRate.Name,
                TaxRateValue = (decimal)account.DefaultTaxRate.Rate
            };
        }

        _logger.LogInformation("Account retrieved: {AccountId} - {Name}", account.Id, account.Name);

        return dto;
    }
}
