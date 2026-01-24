using QIMy.Application.Common.Models;

namespace QIMy.Application.TaxRates.DTOs;

public class VatRateDto
{
    public int Id { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string RateType { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveUntil { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
