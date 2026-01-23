using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Core.Entities;

namespace QIMy.Application.Businesses.Commands.DeleteBusiness;

public class DeleteBusinessCommandHandler : IRequestHandler<DeleteBusinessCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteBusinessCommandHandler> _logger;

    public DeleteBusinessCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteBusinessCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteBusinessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting business: Id={BusinessId}", request.BusinessId);

        var business = await _unitOfWork.Businesses.GetByIdAsync(request.BusinessId, cancellationToken);

        if (business == null)
        {
            throw new NotFoundException(nameof(QIMy.Core.Entities.Business), request.BusinessId);
        }

        // Check if business has related invoices
        var hasInvoices = await _unitOfWork.Invoices.CountAsync(i => i.BusinessId == request.BusinessId && !i.IsDeleted, cancellationToken);

        if (hasInvoices > 0)
        {
            return Result.Failure($"Cannot delete business. It has {hasInvoices} associated invoice(s).");
        }

        await _unitOfWork.Businesses.DeleteAsync(request.BusinessId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Business deleted successfully: Id={BusinessId}", request.BusinessId);

        return Result.Success();
    }
}
