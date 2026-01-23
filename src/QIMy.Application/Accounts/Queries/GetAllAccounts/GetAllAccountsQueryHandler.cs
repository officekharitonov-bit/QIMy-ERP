using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Accounts.DTOs;
using QIMy.Application.Common.Interfaces;

namespace QIMy.Application.Accounts.Queries.GetAllAccounts;

public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, IEnumerable<AccountDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllAccountsQueryHandler> _logger;

    public GetAllAccountsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllAccountsQueryHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._logger = _logger;
    }

    public async Task<IEnumerable<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all accounts");

        var accounts = await _unitOfWork.Accounts.GetAllAsync(cancellationToken);

        var accountDtos = new List<AccountDto>();

        foreach (var account in accounts.OrderBy(a => a.AccountNumber))
        {
            var dto = _mapper.Map<AccountDto>(account);

            if (account.DefaultTaxRate != null)
            {
                dto = dto with
                {
                    TaxRateName = account.DefaultTaxRate.Name,
                    TaxRateValue = (decimal)account.DefaultTaxRate.Rate
                };
            }

            accountDtos.Add(dto);
        }

        _logger.LogInformation("Retrieved {Count} accounts", accountDtos.Count);

        return accountDtos;
    }
}
