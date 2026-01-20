using System.Net.Http;
using System.Text.RegularExpressions;
using QIMy.Core.Interfaces;

namespace QIMy.Infrastructure.Services;

public class ViesService : IViesService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private const string ViesUrl = "https://ec.europa.eu/taxation_customs/vies/rest-api/ms";
    
    public async Task<ViesResponse?> CheckVatNumberAsync(string countryCode, string vatNumber)
    {
        try
        {
            // Очистка VAT номера от пробелов и спецсимволов
            vatNumber = CleanVatNumber(vatNumber, countryCode);
            
            if (string.IsNullOrEmpty(vatNumber))
                return null;

            // Формируем URL для REST API VIES
            var fullVatNumber = $"{countryCode.ToUpper()}{vatNumber}";
            var url = $"{ViesUrl}/{countryCode.ToUpper()}/vat/{vatNumber}";
            
            Console.WriteLine($"VIES Request: {url}");

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"VIES HTTP Error: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"VIES Response: {content.Substring(0, Math.Min(200, content.Length))}...");

            // Парсим JSON ответ
            var json = System.Text.Json.JsonDocument.Parse(content);
            var root = json.RootElement;

            if (root.TryGetProperty("isValid", out var isValidElement) && isValidElement.GetBoolean())
            {
                var name = root.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
                var address = root.TryGetProperty("address", out var addressElement) ? addressElement.GetString() : null;

                return new ViesResponse
                {
                    IsValid = true,
                    CompanyName = name?.Trim(),
                    Address = address?.Replace("\n", ", ").Trim(),
                    CountryCode = countryCode.ToUpper(),
                    VatNumber = fullVatNumber
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"VIES API Error: {ex.Message}");
            return null;
        }
    }

    private string CleanVatNumber(string vatNumber, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(vatNumber))
            return string.Empty;

        // Убираем пробелы, дефисы, точки
        vatNumber = vatNumber.Replace(" ", "").Replace("-", "").Replace(".", "");
        
        // Убираем код страны в начале, если он есть
        if (vatNumber.StartsWith(countryCode, StringComparison.OrdinalIgnoreCase))
        {
            vatNumber = vatNumber.Substring(countryCode.Length);
        }
        
        return vatNumber.Trim();
    }
}
