namespace QIMy.Application.Products.DTOs;

public record ProductDto
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
    public string? UnitName { get; init; }
    public string? TaxRateName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateProductDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? SKU { get; init; }
    public decimal Price { get; init; }
    public int? UnitId { get; init; }
    public int? TaxRateId { get; init; }
    public bool IsService { get; init; }
    public int StockQuantity { get; init; }
}

public record UpdateProductDto
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
}
