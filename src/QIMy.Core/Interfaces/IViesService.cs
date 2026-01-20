namespace QIMy.Core.Interfaces;

public interface IViesService
{
    Task<ViesResponse?> CheckVatNumberAsync(string countryCode, string vatNumber);
}

public class ViesResponse
{
    public bool IsValid { get; set; }
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
}
