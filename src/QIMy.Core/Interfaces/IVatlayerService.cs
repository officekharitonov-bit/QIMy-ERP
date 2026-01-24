using QIMy.Core.Entities;

namespace QIMy.Core.Interfaces;

/// <summary>
/// Service for interacting with Vatlayer API
/// </summary>
public interface IVatlayerService
{
    /// <summary>
    /// Get current VAT rates for all EU countries
    /// </summary>
    Task<VatlayerResponse?> GetVatRatesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get VAT rate for specific country
    /// </summary>
    Task<VatlayerCountryRate?> GetCountryRateAsync(string countryCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate VAT number using VIES
    /// </summary>
    Task<bool> ValidateVatNumberAsync(string countryCode, string vatNumber, CancellationToken cancellationToken = default);
}

/// <summary>
/// Response from Vatlayer API
/// </summary>
public class VatlayerResponse
{
    public bool Success { get; set; }
    public Dictionary<string, VatlayerCountryRate> Rates { get; set; } = new();
}

/// <summary>
/// VAT rates for a single country
/// </summary>
public class VatlayerCountryRate
{
    public string CountryName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public decimal StandardRate { get; set; }
    public decimal? ReducedRate { get; set; }
    public decimal? ReducedRate1 { get; set; }
    public decimal? ReducedRate2 { get; set; }
    public decimal? SuperReducedRate { get; set; }
    public decimal? ParkingRate { get; set; }
}
