using MediatR;
using QIMy.Application.Products.DTOs;

namespace QIMy.Application.Products.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>
{
    /// <summary>
    /// Фильтр по бизнесу (опционально)
    /// </summary>
    public int? BusinessId { get; init; }
}
