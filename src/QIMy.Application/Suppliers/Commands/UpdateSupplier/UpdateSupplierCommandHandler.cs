using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Suppliers.DTOs;

namespace QIMy.Application.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Result<SupplierDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSupplierCommandHandler> _logger;
    private readonly IDuplicateDetectionService _duplicateDetectionService;

    public UpdateSupplierCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateSupplierCommandHandler> logger,
        IDuplicateDetectionService duplicateDetectionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _duplicateDetectionService = duplicateDetectionService;
    }

    public async Task<Result<SupplierDto>> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(request.Id, cancellationToken);
        if (supplier == null)
        {
            _logger.LogWarning("Supplier not found with Id: {SupplierId}", request.Id);
            return Result<SupplierDto>.Failure($"Supplier with Id {request.Id} not found.");
        }

        // Проверка безопасности: BusinessId должен совпадать
        if (supplier.BusinessId != request.BusinessId)
        {
            _logger.LogWarning("Unauthorized access attempt: Supplier {SupplierId} belongs to BusinessId {ActualBusinessId}, but request is for BusinessId {RequestBusinessId}",
                request.Id, supplier.BusinessId, request.BusinessId);
            return Result<SupplierDto>.Failure("Access denied: Supplier belongs to another business.");
        }

        // Check for duplicate supplier (excluding current supplier)
        var duplicateResult = await _duplicateDetectionService.CheckSupplierDuplicateAsync(
            request.CompanyName,
            request.VatNumber,
            request.Id,
            cancellationToken);

        if (duplicateResult != null)
        {
            _logger.LogWarning(
                "Duplicate supplier detected during update: {CompanyName}, VatNumber: {VatNumber}. Severity: {Severity}",
                request.CompanyName,
                request.VatNumber,
                duplicateResult.Severity);

            // First attempt - user hasn't acknowledged duplicate
            if (!request.IgnoreDuplicateWarning)
            {
                return Result<SupplierDto>.Failure(
                    $"A supplier with similar details already exists. " +
                    $"If you want to proceed, set IgnoreDuplicateWarning=true and DoubleConfirmed=true to confirm. " +
                    $"Duplicate details: {duplicateResult.Message}");
            }

            // Second attempt - user acknowledged but hasn't double confirmed
            if (!request.DoubleConfirmed)
            {
                return Result<SupplierDto>.Failure(
                    $"Please confirm that you want to update to a duplicate supplier by setting DoubleConfirmed=true. " +
                    $"Duplicate details: {duplicateResult.Message}");
            }

            // User has double confirmed - proceed with warning
            _logger.LogWarning(
                "Updating supplier despite duplicate warning. Id: {SupplierId}, CompanyName: {CompanyName}, User confirmed twice.",
                request.Id,
                request.CompanyName);
        }

        // Update supplier properties
        supplier.BusinessId = request.BusinessId;
        supplier.CompanyName = request.CompanyName;
        supplier.ContactPerson = request.ContactPerson;
        supplier.Email = request.Email;
        supplier.Phone = request.Phone;
        supplier.Address = request.Address;
        supplier.City = request.City;
        supplier.PostalCode = request.PostalCode;
        supplier.Country = request.Country;
        supplier.TaxNumber = request.TaxNumber;
        supplier.VatNumber = request.VatNumber;
        supplier.BankAccount = request.BankAccount;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Suppliers.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Supplier updated successfully with Id: {SupplierId}", supplier.Id);

        var supplierDto = _mapper.Map<SupplierDto>(supplier);
        return Result<SupplierDto>.Success(supplierDto);
    }
}
