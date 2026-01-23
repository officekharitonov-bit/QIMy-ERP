using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QIMy.Application.Common.Interfaces;
using QIMy.Application.Products.DTOs;

namespace QIMy.Application.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;

    public GetAllProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllProductsQueryHandler> _logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        this._logger = _logger;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products");

        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);

        var productDtos = new List<ProductDto>();

        foreach (var product in products.OrderBy(p => p.Name))
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

            productDtos.Add(dto);
        }

        _logger.LogInformation("Retrieved {Count} products", productDtos.Count);

        return productDtos;
    }
}
