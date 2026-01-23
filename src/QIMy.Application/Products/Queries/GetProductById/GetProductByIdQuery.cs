using MediatR;
using QIMy.Application.Products.DTOs;

namespace QIMy.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(int ProductId) : IRequest<ProductDto?>;
