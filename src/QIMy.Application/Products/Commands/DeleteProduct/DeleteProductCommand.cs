using MediatR;
using QIMy.Application.Common.Models;

namespace QIMy.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int ProductId) : IRequest<Result>;
