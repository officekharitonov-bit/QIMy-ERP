namespace QIMy.Application.Currencies.DTOs;

public record CurrencyDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public decimal ExchangeRate { get; init; }
    public bool IsDefault { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateCurrencyDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public decimal ExchangeRate { get; init; }
    public bool IsDefault { get; init; } = false;
}

public record UpdateCurrencyDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public decimal ExchangeRate { get; init; }
    public bool IsDefault { get; init; }
}
