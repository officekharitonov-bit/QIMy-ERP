using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Queries.GetAllVatRates;

/// <summary>
/// Get all current VAT rates (grouped by country)
/// </summary>
public record GetAllVatRatesQuery : IRequest<Result<List<VatRateDto>>>
{
    /// <summary>
    /// Include historical (inactive) rates
    /// </summary>
    public bool IncludeHistorical { get; init; } = false;

    /// <summary>
    /// Filter by country code
    /// </summary>
    public string? CountryCode { get; init; }
}
