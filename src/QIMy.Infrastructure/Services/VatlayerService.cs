using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QIMy.Core.Interfaces;

namespace QIMy.Infrastructure.Services;

/// <summary>
/// Service for interacting with Vatlayer API
/// Documentation: https://vatlayer.com/documentation
/// </summary>
public class VatlayerService : IVatlayerService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VatlayerService> _logger;
    private const string BaseUrl = "http://apilayer.net/api";

    public VatlayerService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<VatlayerService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get all VAT rates from Vatlayer API
    /// </summary>
    public async Task<VatlayerResponse?> GetVatRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _configuration["Vatlayer:ApiKey"] ?? "557cbfef011986c43c4ef183647acb99";
            var url = $"{BaseUrl}/rate_list?access_key={apiKey}";

            _logger.LogInformation("Fetching VAT rates from Vatlayer API");

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Vatlayer API returned error: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<VatlayerApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse == null || !apiResponse.Success)
            {
                _logger.LogError("Vatlayer API returned unsuccessful response");
                return null;
            }

            var result = new VatlayerResponse
            {
                Success = true,
                Rates = new Dictionary<string, VatlayerCountryRate>()
            };

            foreach (var kvp in apiResponse.Rates)
            {
                result.Rates[kvp.Key] = new VatlayerCountryRate
                {
                    CountryCode = kvp.Key,
                    CountryName = kvp.Value.CountryName,
                    StandardRate = kvp.Value.StandardRate,
                    ReducedRate = kvp.Value.ReducedRate,
                    ReducedRate1 = kvp.Value.ReducedRate1,
                    ReducedRate2 = kvp.Value.ReducedRate2,
                    SuperReducedRate = kvp.Value.SuperReducedRate,
                    ParkingRate = kvp.Value.ParkingRate
                };
            }

            _logger.LogInformation("Successfully fetched {Count} VAT rates from Vatlayer", result.Rates.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching VAT rates from Vatlayer API");
            return null;
        }
    }

    /// <summary>
    /// Get VAT rate for specific country
    /// </summary>
    public async Task<VatlayerCountryRate?> GetCountryRateAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _configuration["Vatlayer:ApiKey"] ?? "557cbfef011986c43c4ef183647acb99";
            var url = $"{BaseUrl}/rate?access_key={apiKey}&country_code={countryCode}";

            _logger.LogInformation("Fetching VAT rate for {CountryCode} from Vatlayer API", countryCode);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Vatlayer API returned error: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<VatlayerSingleRateResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse == null || !apiResponse.Success)
            {
                _logger.LogError("Vatlayer API returned unsuccessful response for {CountryCode}", countryCode);
                return null;
            }

            return new VatlayerCountryRate
            {
                CountryCode = apiResponse.CountryCode,
                CountryName = apiResponse.CountryName,
                StandardRate = apiResponse.StandardRate,
                ReducedRate = apiResponse.ReducedRate,
                ReducedRate1 = apiResponse.ReducedRate1,
                ReducedRate2 = apiResponse.ReducedRate2,
                SuperReducedRate = apiResponse.SuperReducedRate,
                ParkingRate = apiResponse.ParkingRate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching VAT rate for {CountryCode} from Vatlayer API", countryCode);
            return null;
        }
    }

    /// <summary>
    /// Validate VAT number using VIES integration (if available in Vatlayer plan)
    /// </summary>
    public async Task<bool> ValidateVatNumberAsync(string countryCode, string vatNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _configuration["Vatlayer:ApiKey"] ?? "557cbfef011986c43c4ef183647acb99";
            var url = $"{BaseUrl}/validate?access_key={apiKey}&vat_number={countryCode}{vatNumber}";

            _logger.LogInformation("Validating VAT number {CountryCode}{VatNumber}", countryCode, vatNumber);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Vatlayer validation API returned error: {StatusCode}", response.StatusCode);
                return false;
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<VatlayerValidationResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Valid ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating VAT number {CountryCode}{VatNumber}", countryCode, vatNumber);
            return false;
        }
    }

    #region Internal API Response Models

    private class VatlayerApiResponse
    {
        public bool Success { get; set; }
        public Dictionary<string, VatlayerApiRate> Rates { get; set; } = new();
    }

    private class VatlayerApiRate
    {
        public string CountryName { get; set; } = string.Empty;
        public decimal StandardRate { get; set; }
        public decimal? ReducedRate { get; set; }
        public decimal? ReducedRate1 { get; set; }
        public decimal? ReducedRate2 { get; set; }
        public decimal? SuperReducedRate { get; set; }
        public decimal? ParkingRate { get; set; }
    }

    private class VatlayerSingleRateResponse
    {
        public bool Success { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public decimal StandardRate { get; set; }
        public decimal? ReducedRate { get; set; }
        public decimal? ReducedRate1 { get; set; }
        public decimal? ReducedRate2 { get; set; }
        public decimal? SuperReducedRate { get; set; }
        public decimal? ParkingRate { get; set; }
    }

    private class VatlayerValidationResponse
    {
        public bool Valid { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
    }

    #endregion
}
