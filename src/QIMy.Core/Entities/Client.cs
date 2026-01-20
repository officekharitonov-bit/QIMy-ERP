namespace QIMy.Core.Entities;

/// <summary>
/// Client/Customer entity
/// </summary>
public class Client : BaseEntity
{
    /// <summary>
    /// Уникальный код клиента (автогенерация на основе ClientArea)
    /// Inland: 200000-229999, EU: 230000-259999, ThirdCountry: 260000-299999
    /// </summary>
    public int? ClientCode { get; set; }

    /// <summary>
    /// Тип клиента (B2B, B2C) - reference to ClientType table
    /// </summary>
    public int? ClientTypeId { get; set; }

    /// <summary>
    /// Географическая область (Inland/EU/ThirdCountry) - reference to ClientArea table
    /// </summary>
    public int? ClientAreaId { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public string? VatNumber { get; set; }

    // Navigation properties
    public ClientType? ClientType { get; set; }
    public ClientArea? ClientArea { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
