using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Suppliers.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Result<SupplierDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSupplierCommandHandler> _logger;
    private readonly IDuplicateDetectionService _duplicateDetectionService;

    public CreateSupplierCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateSupplierCommandHandler> logger,
        IDuplicateDetectionService duplicateDetectionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _duplicateDetectionService = duplicateDetectionService;
    }

    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate supplier
        var duplicateResult = await _duplicateDetectionService.CheckSupplierDuplicateAsync(
            request.CompanyName,
            request.VatNumber,
            null,
            cancellationToken);

        if (duplicateResult != null)
        {
            _logger.LogWarning(
                "Duplicate supplier detected: {CompanyName}, VatNumber: {VatNumber}. Severity: {Severity}",
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
                    $"Please confirm that you want to create a duplicate supplier by setting DoubleConfirmed=true. " +
                    $"Duplicate details: {duplicateResult.Message}");
            }

            // User has double confirmed - proceed with warning
            _logger.LogWarning(
                "Creating supplier despite duplicate warning. CompanyName: {CompanyName}, User confirmed twice.",
                request.CompanyName);
        }

        // Create the supplier entity
        var supplier = new Supplier
        {
            BusinessId = request.BusinessId,
            CompanyName = request.CompanyName,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country,
            TaxNumber = request.TaxNumber,
            VatNumber = request.VatNumber,
            BankAccount = request.BankAccount,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Suppliers.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Supplier created successfully with Id: {SupplierId}", supplier.Id);

        var supplierDto = _mapper.Map<SupplierDto>(supplier);
        return Result<SupplierDto>.Success(supplierDto);
    }
}
