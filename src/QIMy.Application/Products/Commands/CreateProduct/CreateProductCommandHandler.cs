using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Products.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductCommandHandler> _logger;
    private readonly IDuplicateDetectionService _duplicateDetectionService;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateProductCommandHandler> _logger,
        IDuplicateDetectionService duplicateDetectionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._logger = _logger;
        _duplicateDetectionService = duplicateDetectionService;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product: {Name}", request.Name);

        // Duplicate check by name/SKU
        var duplicate = await _duplicateDetectionService.CheckProductDuplicateAsync(
            request.Name,
            request.SKU,
            excludeId: null,
            cancellationToken);

        if (duplicate != null)
        {
            if (!request.IgnoreDuplicateWarning)
            {
                return Result<ProductDto>.Failure(
                    $"Duplicate product: {duplicate.Message}. Set IgnoreDuplicateWarning=true and DoubleConfirmed=true to proceed.");
            }

            if (request.IgnoreDuplicateWarning && !request.DoubleConfirmed)
            {
                return Result<ProductDto>.Failure(
                    $"Second confirmation required: {duplicate.Message}. Set DoubleConfirmed=true.");
            }

            _logger.LogWarning("Product duplicate accepted after double confirmation: {Name}", request.Name);
        }

        // Validate Unit exists if provided
        if (request.UnitId.HasValue)
        {
            var unit = await _unitOfWork.Units.GetByIdAsync(request.UnitId.Value, cancellationToken);
            if (unit == null)
            {
                return Result<ProductDto>.Failure("Selected Unit does not exist.");
            }
        }

        // Validate TaxRate exists if provided
        if (request.TaxRateId.HasValue)
        {
            var taxRate = await _unitOfWork.TaxRates.GetByIdAsync(request.TaxRateId.Value, cancellationToken);
            if (taxRate == null)
            {
                return Result<ProductDto>.Failure("Selected Tax Rate does not exist.");
            }
        }

        var product = _mapper.Map<Product>(request);

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var productDto = await MapToDto(product, cancellationToken);

        _logger.LogInformation("Product created successfully: Id={Id}, Name={Name}", product.Id, product.Name);

        return Result<ProductDto>.Success(productDto);
    }

    private async Task<ProductDto> MapToDto(Product product, CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<ProductDto>(product);

        if (product.Unit != null)
        {
            dto = dto with { UnitName = product.Unit.Name };
        }

        if (product.TaxRate != null)
        {
            dto = dto with { TaxRateName = product.TaxRate.Name };
        }

        return await Task.FromResult(dto);
    }
}
