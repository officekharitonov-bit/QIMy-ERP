using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Exceptions;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Common.Models;
using QIMy.Application.Products.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProductCommandHandler> _logger;
    private readonly IDuplicateDetectionService _duplicateDetectionService;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateProductCommandHandler> _logger,
        IDuplicateDetectionService duplicateDetectionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._logger = _logger;
        _duplicateDetectionService = duplicateDetectionService;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product: Id={Id}", request.Id);

        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found.");
        }

        // Проверка безопасности: BusinessId должен совпадать
        if (request.BusinessId.HasValue && product.BusinessId != request.BusinessId.Value)
        {
            _logger.LogWarning("Unauthorized access attempt: Product {ProductId} belongs to BusinessId {ActualBusinessId}, but request is for BusinessId {RequestBusinessId}",
                request.Id, product.BusinessId, request.BusinessId.Value);
            return Result<ProductDto>.Failure($"Access denied: Product belongs to another business.");
        }

        // Duplicate check by name/SKU
        var duplicate = await _duplicateDetectionService.CheckProductDuplicateAsync(
            request.Name,
            request.SKU,
            excludeId: request.Id,
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

            _logger.LogWarning("Product duplicate accepted after double confirmation on update: Id={Id}", request.Id);
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

        _mapper.Map(request, product);
        if (request.BusinessId.HasValue)
        {
            product.BusinessId = request.BusinessId;
        }

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var productDto = await MapToDto(product, cancellationToken);

        _logger.LogInformation("Product updated successfully: Id={Id}, Name={Name}", product.Id, product.Name);

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
