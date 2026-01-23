namespace QIMy.Application.Accounts.DTOs;

public record AccountDto
{
    public int Id { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string AccountCode { get; init; } = string.Empty;
    public int? DefaultTaxRateId { get; init; }
    public string? TaxRateName { get; init; }
    public decimal? TaxRateValue { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateAccountDto
{
    public string AccountNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string AccountCode { get; init; } = string.Empty;
    public int? DefaultTaxRateId { get; init; }
}

public record UpdateAccountDto
{
    public int Id { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string AccountCode { get; init; } = string.Empty;
    public int? DefaultTaxRateId { get; init; }
}
