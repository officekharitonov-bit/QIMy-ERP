using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.Products.DTOs;

namespace QIMy.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand : IRequest<Result<ProductDto>>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? SKU { get; init; }
    public decimal Price { get; init; }
    public int? UnitId { get; init; }
    public int? TaxRateId { get; init; }
    public bool IsService { get; init; }
    public int StockQuantity { get; init; }

    public bool IgnoreDuplicateWarning { get; init; }
    public bool DoubleConfirmed { get; init; }
}
