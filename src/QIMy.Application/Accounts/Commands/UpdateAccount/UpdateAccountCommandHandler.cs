using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Accounts.DTOs;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Accounts.Commands.UpdateAccount;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Result<AccountDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateAccountCommandHandler> _logger;

    public UpdateAccountCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateAccountCommandHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._logger = _logger;
    }

    public async Task<Result<AccountDto>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating account: Id={Id}, AccountNumber={AccountNumber}", request.Id, request.AccountNumber);

        var account = await _unitOfWork.Accounts.GetByIdAsync(request.Id, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException(nameof(QIMy.Core.Entities.Account), request.Id);
        }

        // Check for duplicate AccountNumber (excluding current account)
        if (request.AccountNumber != account.AccountNumber)
        {
            var existingByNumber = await _unitOfWork.Accounts
                .FindAsync(a => a.AccountNumber == request.AccountNumber && !a.IsDeleted && a.Id != request.Id, cancellationToken);

            if (existingByNumber.Any())
            {
                return Result<AccountDto>.Failure("Account with this Account Number already exists.");
            }
        }

        // Check for duplicate AccountCode (excluding current account)
        if (request.AccountCode != account.AccountCode)
        {
            var existingByCode = await _unitOfWork.Accounts
                .FindAsync(a => a.AccountCode == request.AccountCode && !a.IsDeleted && a.Id != request.Id, cancellationToken);

            if (existingByCode.Any())
            {
                return Result<AccountDto>.Failure("Account with this Account Code already exists.");
            }
        }

        // Validate TaxRate exists if provided
        if (request.DefaultTaxRateId.HasValue && request.DefaultTaxRateId != account.DefaultTaxRateId)
        {
            var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(request.DefaultTaxRateId.Value, cancellationToken);
            if (taxRate == null)
            {
                return Result<AccountDto>.Failure("Selected Tax Rate does not exist.");
            }
        }

        _mapper.Map(request, account);

        await _unitOfWork.Accounts.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accountDto = await MapToDto(account, cancellationToken);

        _logger.LogInformation("Account updated successfully: Id={Id}, AccountNumber={AccountNumber}", account.Id, account.AccountNumber);

        return Result<AccountDto>.Success(accountDto);
    }

    private async Task<AccountDto> MapToDto(QIMy.Core.Entities.Account account, CancellationToken cancellationToken)
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
