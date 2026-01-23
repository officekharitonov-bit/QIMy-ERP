using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Products.DTOs;

namespace QIMy.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetProductByIdQueryHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._logger = _logger;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product by ID: {ProductId}", request.ProductId);

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", request.ProductId);
            return null;
        }

        var dto = _mapper.Map<ProductDto>(product);

        if (product.Unit != null)
        {
            dto = dto with { UnitName = product.Unit.Name };
        }

        if (product.TaxRate != null)
        {
            dto = dto with { TaxRateName = product.TaxRate.Name };
        }

        _logger.LogInformation("Product retrieved: {ProductId} - {Name}", product.Id, product.Name);

        return dto;
    }
}
