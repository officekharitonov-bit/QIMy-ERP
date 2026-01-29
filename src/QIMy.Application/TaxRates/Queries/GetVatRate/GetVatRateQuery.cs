using MediatR;
using QIMy.Application.Common.Models;
using QIMy.Application.TaxRates.DTOs;

namespace QIMy.Application.TaxRates.Queries.GetVatRate;

/// <summary>
/// Get VAT rate for specific country and date
/// </summary>
public record GetVatRateQuery : IRequest<Result<VatRateDto>>
{
    /// <summary>
    /// Country code (ISO 3166-1 alpha-2): AT, DE, GB, etc.
    /// </summary>
    public string CountryCode { get; init; } = string.Empty;

    /// <summary>
    /// Date for which to get the rate (defaults to current date)
    /// Used for historical invoices
    /// </summary>
    public DateTime? AsOfDate { get; init; }

    /// <summary>
    /// Type of rate: Standard, Reduced, etc. (defaults to Standard)
    /// </summary>
    public string? RateType { get; init; }
}
