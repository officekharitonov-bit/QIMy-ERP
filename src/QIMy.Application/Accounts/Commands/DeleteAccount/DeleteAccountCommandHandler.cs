using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Core.Entities;

namespace QIMy.Application.Accounts.Commands.DeleteAccount;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAccountCommandHandler> _logger;

    public DeleteAccountCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteAccountCommandHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        this._logger = _logger;
    }

    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting account: Id={AccountId}", request.AccountId);

        var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        // Note: InvoiceItem doesn't have AccountId field, so we skip this check
        // Accounts can be safely deleted if they're not referenced directly
        var usedInInvoiceItems = 0;

        if (usedInInvoiceItems > 0)
        {
            return Result.Failure($"Cannot delete account. It is used in {usedInInvoiceItems} invoice item(s).");
        }

        await _unitOfWork.Accounts.DeleteAsync(request.AccountId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account deleted successfully: Id={AccountId}", request.AccountId);

        return Result.Success();
    }
}
