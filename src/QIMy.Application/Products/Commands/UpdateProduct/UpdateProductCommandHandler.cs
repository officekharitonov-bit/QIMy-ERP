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

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateProductCommandHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._logger = _logger;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product: Id={Id}", request.Id);

        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found.");
        }

        // Check for duplicate SKU if provided and changed
        if (!string.IsNullOrEmpty(request.SKU) && request.SKU != product.SKU)
        {
            var existingSku = await _unitOfWork.Products
                .FindAsync(p => p.SKU == request.SKU && p.Id != request.Id && !p.IsDeleted, cancellationToken);

            if (existingSku.Any())
            {
                return Result<ProductDto>.Failure("Product with this SKU already exists.");
            }
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
