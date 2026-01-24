namespace QIMy.Application.TaxRates.DTOs;

public record TaxRateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Rate { get; init; }
    public string RateType { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateTaxRateDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Rate { get; init; }
    public bool IsActive { get; init; } = true;
}

public record UpdateTaxRateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Rate { get; init; }
    public bool IsActive { get; init; }
}
