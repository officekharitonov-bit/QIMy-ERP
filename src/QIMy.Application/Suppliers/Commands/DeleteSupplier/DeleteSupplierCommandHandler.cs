using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Suppliers.Commands.DeleteSupplier;

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSupplierCommandHandler> _logger;

    public DeleteSupplierCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteSupplierCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(request.Id, cancellationToken);
        if (supplier == null)
        {
            _logger.LogWarning("Supplier not found with Id: {SupplierId}", request.Id);
            return Result<bool>.Failure($"Supplier with Id {request.Id} not found.");
        }

        // Check if supplier has related expense invoices
        var expenseInvoices = await _unitOfWork.ExpenseInvoices.FindAsync(
            ei => ei.SupplierId == request.Id, cancellationToken);

        if (expenseInvoices.Any())
        {
            _logger.LogWarning("Cannot delete supplier {SupplierId} because it has related expense invoices.", request.Id);
            return Result<bool>.Failure(
                $"Cannot delete supplier '{supplier.CompanyName}' because it has related expense invoices. " +
                "Please delete or reassign the expense invoices first.");
        }

        await _unitOfWork.Suppliers.DeleteAsync(request.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Supplier deleted successfully with Id: {SupplierId}", request.Id);

        return Result<bool>.Success(true);
    }
}
