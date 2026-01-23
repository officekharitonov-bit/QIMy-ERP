using MediatR;
using QIMy.Application.Products.DTOs;

namespace QIMy.Application.Products.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>;
