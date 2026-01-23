using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Accounts.DTOs;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Core.Entities;

namespace QIMy.Application.Accounts.Commands.CreateAccount;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<AccountDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateAccountCommandHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._logger = _logger;
    }

    public async Task<Result<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating account: {AccountNumber} - {Name}", request.AccountNumber, request.Name);

        // Check for duplicate AccountNumber
        var existingByNumber = await _unitOfWork.Accounts
            .FindAsync(a => a.AccountNumber == request.AccountNumber && !a.IsDeleted, cancellationToken);

        if (existingByNumber.Any())
        {
            return Result<AccountDto>.Failure("Account with this Account Number already exists.");
        }

        // Check for duplicate AccountCode
        var existingByCode = await _unitOfWork.Accounts
            .FindAsync(a => a.AccountCode == request.AccountCode && !a.IsDeleted, cancellationToken);

        if (existingByCode.Any())
        {
            return Result<AccountDto>.Failure("Account with this Account Code already exists.");
        }

        // Validate TaxRate exists if provided
        if (request.DefaultTaxRateId.HasValue)
        {
            var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(request.DefaultTaxRateId.Value, cancellationToken);
            if (taxRate == null)
            {
                return Result<AccountDto>.Failure("Selected Tax Rate does not exist.");
            }
        }

        var account = _mapper.Map<Account>(request);

        await _unitOfWork.Accounts.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accountDto = await MapToDto(account, cancellationToken);

        _logger.LogInformation("Account created successfully: Id={Id}, AccountNumber={AccountNumber}", account.Id, account.AccountNumber);

        return Result<AccountDto>.Success(accountDto);
    }

    private async Task<AccountDto> MapToDto(Account account, CancellationToken cancellationToken)
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

        return await Task.FromResult(dto);
    }
}
